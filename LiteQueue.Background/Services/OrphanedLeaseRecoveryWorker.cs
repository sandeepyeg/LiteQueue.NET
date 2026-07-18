using LiteQueue.Infrastructure.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LiteQueue.Background.Services;

public class OrphanedLeaseRecoveryWorker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<OrphanedLeaseRecoveryWorker> _logger;

    public OrphanedLeaseRecoveryWorker(
        IConnectionMultiplexer redis,
        ILogger<OrphanedLeaseRecoveryWorker> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrphanedLeaseRecoveryWorker started.");

        var db = _redis.GetDatabase();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var receiptKeys = server.Keys(pattern: "litequeue:receipt:*").ToArray();

                foreach (var receiptKey in receiptKeys)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var messageIdValue = await db.StringGetAsync(receiptKey);
                    if (messageIdValue.IsNullOrEmpty)
                    {
                        await db.KeyDeleteAsync(receiptKey);
                        continue;
                    }

                    var messageKey = RedisKeyBuilder.MessageKey(messageIdValue.ToString());
                    var exists = await db.KeyExistsAsync(messageKey);

                    if (!exists)
                    {
                        await db.KeyDeleteAsync(receiptKey);
                        _logger.LogDebug("Orphaned receipt cleaned: {ReceiptKey}", receiptKey.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning orphaned leases.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }

        _logger.LogInformation("OrphanedLeaseRecoveryWorker stopped.");
    }
}
