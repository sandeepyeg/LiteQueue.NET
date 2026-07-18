using LiteQueue.Domain.Interfaces;
using LiteQueue.Infrastructure.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LiteQueue.Background.Services;

public class VisibilityTimeoutWorker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<VisibilityTimeoutWorker> _logger;

    public VisibilityTimeoutWorker(
        IConnectionMultiplexer redis,
        ILogger<VisibilityTimeoutWorker> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VisibilityTimeoutWorker started.");

        var db = _redis.GetDatabase();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var inflightKeys = server.Keys(pattern: "litequeue:queue:*:inflight").ToArray();
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                foreach (var key in inflightKeys)
                {
                    var expired = await db.SortedSetRangeByScoreAsync(key, double.NegativeInfinity, now);

                    foreach (var entry in expired)
                    {
                        await db.SortedSetRemoveAsync(key, entry);

                        var queueName = key.ToString().Split(':')[2];

                        var messageKey = RedisKeyBuilder.MessageKey(entry.ToString());
                        var exists = await db.KeyExistsAsync(messageKey);

                        if (exists)
                        {
                            await db.ListLeftPushAsync(RedisKeyBuilder.QueueReadyKey(queueName), entry);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning visibility timeouts.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }

        _logger.LogInformation("VisibilityTimeoutWorker stopped.");
    }
}
