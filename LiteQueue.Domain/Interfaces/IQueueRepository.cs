using LiteQueue.Domain.Models;

namespace LiteQueue.Domain.Interfaces;

public interface IQueueRepository
{
    Task CreateQueueAsync(string queueName);
    Task<IEnumerable<string>> ListQueuesAsync();
    Task SendMessageAsync(string queueName, QueueMessage message);
    Task SendMessageWithDelayAsync(string queueName, QueueMessage message, TimeSpan delay);
    Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(string queueName, int maxMessages, TimeSpan visibilityTimeout);
    Task<IEnumerable<QueueMessage>> ReceiveWithLongPollingAsync(string queueName, int maxMessages, TimeSpan visibilityTimeout, TimeSpan longPollTimeout);
    Task AcknowledgeMessageAsync(string queueName, string receiptHandle);
    Task RejectMessageAsync(string queueName, string receiptHandle);
    Task<QueueMessage?> PeekMessageAsync(string queueName);
    Task<IEnumerable<DeadLetterMessage>> GetDeadLetterMessagesAsync(string queueName);
    Task RedriveDeadLetterMessageAsync(string queueName, string messageId);
    Task<bool> DeleteDeadLetterMessageAsync(string queueName, string messageId);
    Task<string?> GetDeduplicationMessageIdAsync(string queueName, string idempotencyKey);
    Task StoreDeduplicationKeyAsync(string queueName, string idempotencyKey, string messageId);
    Task<QueueStats> GetQueueStatisticsAsync(string queueName);
    Task PurgeQueueAsync(string queueName);
    Task PauseQueueAsync(string queueName);
    Task ResumeQueueAsync(string queueName);
    Task<QueueDefinition?> GetQueueConfigurationAsync(string queueName);
    Task UpdateQueueConfigurationAsync(string queueName, QueueDefinition config);
    Task ExtendVisibilityAsync(string queueName, string receiptHandle, TimeSpan visibilityTimeout);
}

public class QueueStats
{
    public long ReadyCount { get; set; }
    public long InFlightCount { get; set; }
    public long DelayedCount { get; set; }
    public long DeadLetterCount { get; set; }
}
