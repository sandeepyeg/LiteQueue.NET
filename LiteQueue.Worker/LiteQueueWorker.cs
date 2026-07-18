using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiteQueue.Worker;

public class LiteQueueWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<LiteQueueWorkerOptions> _options;
    private readonly ILogger<LiteQueueWorker> _logger;

    private sealed record ReceivedMessage(string MessageId, string ReceiptHandle, string Body, int DeliveryCount);
    private sealed record AcknowledgePayload(string ReceiptHandle);
    private sealed record RejectPayload(string ReceiptHandle);

    public LiteQueueWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<LiteQueueWorkerOptions> options,
        ILogger<LiteQueueWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _options.Value;
        using var semaphore = new SemaphoreSlim(options.MaxConcurrentMessages);

        _logger.LogInformation(
            "LiteQueueWorker started for queue {QueueName}, max concurrent: {Max}",
            options.QueueName, options.MaxConcurrentMessages);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await semaphore.WaitAsync(stoppingToken);

                var messages = await PollAsync(options, stoppingToken);

                if (messages.Length > 0)
                {
                    foreach (var msg in messages)
                    {
                        _ = ProcessMessageAsync(msg, options, stoppingToken)
                            .ContinueWith(_ => semaphore.Release(), TaskScheduler.Default);
                    }
                }
                else
                {
                    semaphore.Release();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LiteQueueWorker poll loop for queue {QueueName}", options.QueueName);
                semaphore.Release();
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    private async Task<ReceivedMessage[]> PollAsync(LiteQueueWorkerOptions options, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("LiteQueueWorker");

        var payload = new
        {
            max = options.MaxConcurrentMessages,
            visibilityTimeout = (int)options.VisibilityTimeout.TotalSeconds,
            longPollTimeout = (int)options.LongPollTimeout.TotalSeconds
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/queues/{options.QueueName}/receive/longpoll")
        {
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
            request.Headers.TryAddWithoutValidation("X-Api-Key", options.ApiKey);

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<ReceivedMessage>>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        return result?.ToArray() ?? Array.Empty<ReceivedMessage>();
    }

    private async Task ProcessMessageAsync(ReceivedMessage message, LiteQueueWorkerOptions options, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        try
        {
            var handlerType = typeof(ILiteQueueHandler<>);
            var handlerInterface = FindHandlerInterface(handlerType, scope.ServiceProvider);
            if (handlerInterface == null)
            {
                _logger.LogWarning("No handler registered for message type enqueue. MessageId: {Id}", message.MessageId);
                await AcknowledgeAsync(options, message.ReceiptHandle, ct);
                return;
            }

            var handler = scope.ServiceProvider.GetService(handlerInterface);
            if (handler == null)
            {
                _logger.LogWarning("Could not resolve handler {HandlerType}", handlerInterface.FullName);
                await AcknowledgeAsync(options, message.ReceiptHandle, ct);
                return;
            }

            var messageType = handlerInterface.GetGenericArguments()[0];
            var messageBody = JsonSerializer.Deserialize(message.Body, messageType,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (messageBody == null)
            {
                _logger.LogWarning("Failed to deserialize message body. MessageId: {Id}", message.MessageId);
                await AcknowledgeAsync(options, message.ReceiptHandle, ct);
                return;
            }

            var handleMethod = handlerInterface.GetMethod("HandleAsync");
            if (handleMethod == null)
            {
                await AcknowledgeAsync(options, message.ReceiptHandle, ct);
                return;
            }

            var task = (Task)handleMethod.Invoke(handler, new object[] { messageBody, ct })!;
            await task;

            await AcknowledgeAsync(options, message.ReceiptHandle, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId} from queue {QueueName}",
                message.MessageId, options.QueueName);
            try
            {
                await RejectAsync(options, message.ReceiptHandle, ct);
            }
            catch (Exception rejectEx)
            {
                _logger.LogError(rejectEx, "Failed to reject message {MessageId}", message.MessageId);
            }
        }
    }

    private static Type? FindHandlerInterface(Type handlerType, IServiceProvider sp)
    {
        foreach (var service in GetAllRegisteredServices(sp))
        {
            foreach (var iface in service.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == handlerType)
                    return iface;
            }
        }
        return null;
    }

    private static IEnumerable<Type> GetAllRegisteredServices(IServiceProvider sp)
    {
        var scope = sp as IServiceScope;
        var provider = scope?.ServiceProvider ?? sp;
        return Enumerable.Empty<Type>();
    }

    private async Task AcknowledgeAsync(LiteQueueWorkerOptions options, string receiptHandle, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("LiteQueueWorker");

        var payload = new AcknowledgePayload(receiptHandle);
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/queues/{options.QueueName}/acknowledge")
        {
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
            request.Headers.TryAddWithoutValidation("X-Api-Key", options.ApiKey);

        await httpClient.SendAsync(request, ct);
    }

    private async Task RejectAsync(LiteQueueWorkerOptions options, string receiptHandle, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("LiteQueueWorker");

        var payload = new RejectPayload(receiptHandle);
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/queues/{options.QueueName}/reject")
        {
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
            request.Headers.TryAddWithoutValidation("X-Api-Key", options.ApiKey);

        await httpClient.SendAsync(request, ct);
    }
}
