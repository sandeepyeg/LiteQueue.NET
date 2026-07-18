using LiteQueue.Infrastructure.Utils;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace LiteQueue.Background.Services;

public class MessageCleaner : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;

    public MessageCleaner(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = _redis.GetDatabase();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());

                var inflightKeys = server.Keys(pattern: "litequeue:queue:*:inflight").ToArray();
                foreach (var key in inflightKeys)
                {
                    var expired = await db.SortedSetRangeByScoreAsync(key, double.NegativeInfinity, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    foreach (var entry in expired)
                    {
                        await db.SortedSetRemoveAsync(key, entry);
                        var queueName = key.ToString().Split(':')[2];
                        await db.ListLeftPushAsync(RedisKeyBuilder.QueueReadyKey(queueName), entry);
                    }
                }
            }
            catch
            {
                // Suppress errors during recovery scan
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
