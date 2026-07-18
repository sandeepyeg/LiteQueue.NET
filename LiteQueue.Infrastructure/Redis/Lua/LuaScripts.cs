namespace LiteQueue.Infrastructure.Redis.Lua;

public static class LuaScripts
{
    public const string ReserveMessageScript = """
        local readyKey = KEYS[1]
        local inflightKey = KEYS[2]
        local messageKey = KEYS[3]
        local receiptKey = KEYS[4]

        local receiptHandle = ARGV[1]
        local visibilityScore = ARGV[2]
        local messageId = ARGV[3]

        local value = redis.call('RPOP', readyKey)
        if not value then
            return 0
        end

        redis.call('ZADD', inflightKey, visibilityScore, value)
        redis.call('SETEX', receiptKey, tonumber(tonumber(visibilityScore) - redis.call('TIME')[1]), value)

        return 1
    """;

    public const string AcknowledgeMessageScript = """
        local inflightKey = KEYS[1]
        local receiptKey = KEYS[2]
        local messageKey = KEYS[3]
        local messageId = ARGV[1]

        redis.call('ZREM', inflightKey, messageId)
        redis.call('DEL', receiptKey)
        redis.call('DEL', messageKey)

        return 1
    """;

    public const string RejectMessageScript = """
        local inflightKey = KEYS[1]
        local receiptKey = KEYS[2]
        local messageKey = KEYS[3]
        local deadKey = KEYS[4]
        local readyKey = KEYS[5]

        local messageId = ARGV[1]
        local maxAttempts = tonumber(ARGV[2])
        local currentDeliveryCount = tonumber(ARGV[3])

        redis.call('ZREM', inflightKey, messageId)
        redis.call('DEL', receiptKey)

        if currentDeliveryCount >= maxAttempts then
            local body = redis.call('GET', messageKey)
            if body then
                redis.call('LPUSH', deadKey, body)
            end
            redis.call('DEL', messageKey)
        else
            redis.call('LPUSH', readyKey, messageId)
        end

        return 1
    """;

    public const string PromoteDelayedScript = """
        local delayedKey = KEYS[1]
        local readyKey = KEYS[2]

        local now = redis.call('TIME')[1]
        local due = redis.call('ZRANGEBYSCORE', delayedKey, 0, now)

        for _, messageId in ipairs(due) do
            redis.call('ZREM', delayedKey, messageId)
            redis.call('LPUSH', readyKey, messageId)
        end

        return #due
    """;

    public const string PublishToSubscriptionsScript = """
        local subscriptionsKey = KEYS[1]

        local subscriptionNames = redis.call('SMEMBERS', subscriptionsKey)

        for _, subName in ipairs(subscriptionNames) do
            local readyKey = 'litequeue:subscription:' .. KEYS[2] .. ':' .. subName .. ':ready'
            redis.call('LPUSH', readyKey, ARGV[1])
        end

        return #subscriptionNames
    """;
}
