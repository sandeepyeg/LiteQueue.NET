namespace LiteQueue.Infrastructure.Utils;

public static class RedisKeyBuilder
{
    public static string QueueKey(string queueName) => $"queue:{queueName}";
    public static string DeadLetterKey(string queueName) => $"dead:{queueName}";
}