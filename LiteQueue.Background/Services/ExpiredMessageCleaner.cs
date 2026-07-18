using LiteQueue.Infrastructure.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LiteQueue.Background.Services;

public class ExpiredMessageCleaner : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ExpiredMessageCleaner> _logger;

    public ExpiredMessageCleaner(
        IConnectionMultiplexer redis,
        ILogger<ExpiredMessageCleaner> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredMessageCleaner started.");

        var db = _redis.GetDatabase();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var messageKeys = server.Keys(pattern: "litequeue:message:*").ToArray();

                foreach (var key in messageKeys)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var ttl = await db.KeyTimeToLiveAsync(key);
                    if (ttl == null || ttl.Value.TotalSeconds > 0) continue;

                    var messageId = key.ToString().Split(':')[2];
                    await db.KeyDeleteAsync(key);
                    _logger.LogDebug("Expired message cleaned: {MessageId}", messageId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning expired messages.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }

        _logger.LogInformation("ExpiredMessageCleaner stopped.");
    }
}
