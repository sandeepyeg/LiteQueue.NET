using System.Text.Json;
using LiteQueue.Domain.Interfaces;
using LiteQueue.Domain.Models;
using LiteQueue.Infrastructure.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LiteQueue.Background.Services;

public class DeadLetterRetentionWorker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IQueueRepository _queueRepository;
    private readonly ILogger<DeadLetterRetentionWorker> _logger;

    public DeadLetterRetentionWorker(
        IConnectionMultiplexer redis,
        IQueueRepository queueRepository,
        ILogger<DeadLetterRetentionWorker> logger)
    {
        _redis = redis;
        _queueRepository = queueRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeadLetterRetentionWorker started.");

        var db = _redis.GetDatabase();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var queues = await _queueRepository.ListQueuesAsync();

                foreach (var queueName in queues)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var config = await _queueRepository.GetQueueConfigurationAsync(queueName);
                    if (config == null) continue;

                    var retentionCutoff = DateTimeOffset.UtcNow.AddSeconds(-config.DeadLetterRetentionSeconds);
                    var deadKey = RedisKeyBuilder.DeadLetterKey(queueName);

                    var deadMessages = await db.ListRangeAsync(deadKey);
                    var removalIndices = new List<long>();

                    for (long i = 0; i < deadMessages.Length; i++)
                    {
                        var deadMsg = JsonSerializer.Deserialize<DeadLetterMessage>(deadMessages[i]!.ToString());
                        if (deadMsg != null && deadMsg.DeadLetteredAt < retentionCutoff)
                        {
                            removalIndices.Add(i);
                        }
                    }

                    removalIndices.Reverse();
                    foreach (var index in removalIndices)
                    {
                        var value = await db.ListGetByIndexAsync(deadKey, index);
                        if (!value.IsNullOrEmpty)
                        {
                            await db.ListRemoveAsync(deadKey, value);
                        }
                    }

                    if (removalIndices.Count > 0)
                    {
                        _logger.LogInformation(
                            "Removed {Count} expired DLQ messages from queue {QueueName}.",
                            removalIndices.Count, queueName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning dead letter messages.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }

        _logger.LogInformation("DeadLetterRetentionWorker stopped.");
    }
}
