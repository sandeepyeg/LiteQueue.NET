namespace DefaultNamespace;

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
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: "queue:*").ToArray();

            foreach (var key in keys)
            {
                var length = await db.ListLengthAsync(key);
                for (long i = 0; i < length; i++)
                {
                    var item = await db.ListGetByIndexAsync(key, i);
                    if (!item.HasValue) continue;

                    var msg = JsonSerializer.Deserialize<QueueMessageDto>(item!)!;
                    if (msg.ExpireAt.HasValue && msg.ExpireAt < DateTimeOffset.UtcNow)
                    {
                        await db.ListRemoveAsync(key, item);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}