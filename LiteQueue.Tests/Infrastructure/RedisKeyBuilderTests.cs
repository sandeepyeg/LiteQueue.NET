using LiteQueue.Infrastructure.Utils;

namespace LiteQueue.Tests.Infrastructure;

public class RedisKeyBuilderTests
{
    [Fact]
    public void QueuesRegistryKey_UsesPrefix()
    {
        Assert.Equal("litequeue:queues", RedisKeyBuilder.QueuesRegistryKey());
    }

    [Fact]
    public void QueueReadyKey_IncludesQueueName()
    {
        Assert.Equal("litequeue:queue:orders:ready", RedisKeyBuilder.QueueReadyKey("orders"));
    }

    [Fact]
    public void QueueInFlightKey_IncludesQueueName()
    {
        Assert.Equal("litequeue:queue:orders:inflight", RedisKeyBuilder.QueueInFlightKey("orders"));
    }

    [Fact]
    public void QueueDelayedKey_IncludesQueueName()
    {
        Assert.Equal("litequeue:queue:orders:delayed", RedisKeyBuilder.QueueDelayedKey("orders"));
    }

    [Fact]
    public void MessageKey_IncludesMessageId()
    {
        Assert.Equal("litequeue:message:msg-123", RedisKeyBuilder.MessageKey("msg-123"));
    }

    [Fact]
    public void ReceiptKey_IncludesReceiptHandle()
    {
        Assert.Equal("litequeue:receipt:rh-xyz", RedisKeyBuilder.ReceiptKey("rh-xyz"));
    }

    [Fact]
    public void DeadLetterKey_IncludesQueueName()
    {
        Assert.Equal("litequeue:queue:orders:dead", RedisKeyBuilder.DeadLetterKey("orders"));
    }

    [Fact]
    public void TopicsRegistryKey_UsesPrefix()
    {
        Assert.Equal("litequeue:topics", RedisKeyBuilder.TopicsRegistryKey());
    }

    [Fact]
    public void TopicSubscriptionsKey_IncludesTopicName()
    {
        Assert.Equal("litequeue:topic:events:subscriptions", RedisKeyBuilder.TopicSubscriptionsKey("events"));
    }

    [Fact]
    public void DedupeKey_IncludesQueueAndIdempotencyKey()
    {
        Assert.Equal("litequeue:dedupe:payments:key-99", RedisKeyBuilder.DedupeKey("payments", "key-99"));
    }

    [Fact]
    public void SchedulePendingKey_UsesPrefix()
    {
        Assert.Equal("litequeue:schedules:pending", RedisKeyBuilder.SchedulePendingKey());
    }

    [Fact]
    public void RecurringJobsKey_UsesPrefix()
    {
        Assert.Equal("litequeue:recurring", RedisKeyBuilder.RecurringJobsKey());
    }

    [Fact]
    public void RecurringJobKey_IncludesJobId()
    {
        Assert.Equal("litequeue:recurring:daily-report", RedisKeyBuilder.RecurringJobKey("daily-report"));
    }
}
