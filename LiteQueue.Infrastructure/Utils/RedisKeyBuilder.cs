namespace LiteQueue.Infrastructure.Utils;

public static class RedisKeyBuilder
{
    private const string Prefix = "litequeue";

    public static string QueuesRegistryKey() => $"{Prefix}:queues";

    public static string QueueConfigKey(string queueName) => $"{Prefix}:queue:{queueName}:config";

    public static string QueueReadyKey(string queueName) => $"{Prefix}:queue:{queueName}:ready";

    public static string QueueInFlightKey(string queueName) => $"{Prefix}:queue:{queueName}:inflight";

    public static string QueueDelayedKey(string queueName) => $"{Prefix}:queue:{queueName}:delayed";

    public static string MessageKey(string messageId) => $"{Prefix}:message:{messageId}";

    public static string ReceiptKey(string receiptHandle) => $"{Prefix}:receipt:{receiptHandle}";

    public static string DeadLetterKey(string queueName) => $"{Prefix}:queue:{queueName}:dead";

    public static string TopicsRegistryKey() => $"{Prefix}:topics";

    public static string TopicSubscriptionsKey(string topicName) => $"{Prefix}:topic:{topicName}:subscriptions";

    public static string SubscriptionQueueKey(string topicName, string subscriptionName) => $"{Prefix}:subscription:{topicName}:{subscriptionName}:ready";

    public static string SchedulePendingKey() => $"{Prefix}:schedules:pending";

    public static string RecurringJobsKey() => $"{Prefix}:recurring";

    public static string RecurringJobKey(string jobId) => $"{Prefix}:recurring:{jobId}";

    public static string DedupeKey(string queueName, string idempotencyKey) => $"{Prefix}:dedupe:{queueName}:{idempotencyKey}";
}
