using LiteQueue.Contracts.Constants.DTOs;

namespace LiteQueue.Domain.Interfaces;


public interface IQueueRepository
{
    Task CreateQueueAsync(string queueName);
    Task<IEnumerable<string>> ListQueuesAsync();
    Task SendMessageAsync(string queueName, QueueMessageDto message);
    Task<IEnumerable<QueueMessageDto>> ReceiveMessagesAsync(string queueName, int maxMessages, TimeSpan visibilityTimeout);
    Task DeleteMessageAsync(string queueName, string messageId);
    Task<QueueMessageDto?> PeekMessageAsync(string queueName);
}