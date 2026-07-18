using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace LiteQueue.Client;

public class LiteQueueClient
{
    private readonly HttpClient _httpClient;
    private readonly LiteQueueClientOptions _options;

    public LiteQueueClient(HttpClient httpClient, IOptions<LiteQueueClientOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<HttpResponseMessage> SendAsync(string queueName, object message, CancellationToken ct = default)
    {
        var content = SerializeContent(message);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/queues/{queueName}/messages")
        {
            Content = content
        };
        ApplyApiKey(request);
        ApplyTimeout(ct);

        return await _httpClient.SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage> ScheduleAsync(string queueName, object message, DateTimeOffset executeAt, CancellationToken ct = default)
    {
        var payload = new { queueName, body = JsonSerializer.Serialize(message), executeAt = executeAt.ToString("O") };
        var content = SerializeContent(payload);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/scheduling/schedules")
        {
            Content = content
        };
        ApplyApiKey(request);
        ApplyTimeout(ct);

        return await _httpClient.SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage> PublishAsync(string topicName, object message, CancellationToken ct = default)
    {
        var content = SerializeContent(message);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/topics/{topicName}/messages")
        {
            Content = content
        };
        ApplyApiKey(request);
        ApplyTimeout(ct);

        return await _httpClient.SendAsync(request, ct);
    }

    private void ApplyApiKey(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            request.Headers.TryAddWithoutValidation("X-Api-Key", _options.ApiKey);
    }

    private void ApplyTimeout(CancellationToken ct)
    {
        if (!ct.CanBeCanceled && _options.DefaultTimeout > TimeSpan.Zero)
            _httpClient.Timeout = _options.DefaultTimeout;
    }

    private static StringContent SerializeContent(object message)
    {
        var json = JsonSerializer.Serialize(message);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
