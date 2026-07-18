using LiteQueue.Domain.Interfaces;
using StackExchange.Redis;

namespace LiteQueue.Application.Services;

public class MessageDeduplicationService
{
    private readonly IQueueRepository _queueRepository;
    private readonly IConnectionMultiplexer _redis;
    private static readonly TimeSpan DefaultDedupeTtl = TimeSpan.FromMinutes(10);

    public MessageDeduplicationService(IQueueRepository queueRepository, IConnectionMultiplexer redis)
    {
        _queueRepository = queueRepository;
        _redis = redis;
    }

    public async Task<string?> TryDeduplicateAsync(string queueName, string idempotencyKey)
    {
        var redisKey = $"litequeue:dedupe:{queueName}:{idempotencyKey}";
        var db = _redis.GetDatabase();

        var existingValue = await db.StringGetAsync(redisKey);
        if (!existingValue.IsNullOrEmpty)
            return existingValue.ToString();

        return null;
    }

    public async Task StoreIdempotencyKeyAsync(string queueName, string idempotencyKey, string messageId)
    {
        var redisKey = $"litequeue:dedupe:{queueName}:{idempotencyKey}";
        var db = _redis.GetDatabase();

        await db.StringSetAsync(redisKey, messageId, DefaultDedupeTtl);
    }
}
