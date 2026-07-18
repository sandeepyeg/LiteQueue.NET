using LiteQueue.Domain.Models;

namespace LiteQueue.Domain.Interfaces;

public interface IQueueRepository
{
    Task CreateQueueAsync(string queueName);
    Task<IEnumerable<string>> ListQueuesAsync();
    Task SendMessageAsync(string queueName, QueueMessage message);
    Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(string queueName, int maxMessages, TimeSpan visibilityTimeout);
    Task AcknowledgeMessageAsync(string queueName, string receiptHandle);
    Task RejectMessageAsync(string queueName, string receiptHandle);
    Task<QueueMessage?> PeekMessageAsync(string queueName);
    Task<IEnumerable<DeadLetterMessage>> GetDeadLetterMessagesAsync(string queueName);
    Task RedriveDeadLetterMessageAsync(string queueName, string messageId);
}
