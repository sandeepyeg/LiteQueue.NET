using LiteQueue.Domain.Interfaces;
using LiteQueue.Domain.Models;

namespace LiteQueue.Application.Services;

public class QueueService
{
    private readonly IQueueRepository _repository;

    public QueueService(IQueueRepository repository)
    {
        _repository = repository;
    }

    public Task CreateQueueAsync(string queueName) => _repository.CreateQueueAsync(queueName);

    public Task<IEnumerable<string>> ListQueuesAsync() => _repository.ListQueuesAsync();

    public Task SendMessageAsync(string queueName, QueueMessage message) =>
        _repository.SendMessageAsync(queueName, message);

    public Task SendMessageWithDelayAsync(string queueName, QueueMessage message, TimeSpan delay) =>
        _repository.SendMessageWithDelayAsync(queueName, message, delay);

    public Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(string queueName, int max, TimeSpan timeout) =>
        _repository.ReceiveMessagesAsync(queueName, max, timeout);

    public Task<IEnumerable<QueueMessage>> ReceiveWithLongPollingAsync(string queueName, int max, TimeSpan visibilityTimeout, TimeSpan longPollTimeout) =>
        _repository.ReceiveWithLongPollingAsync(queueName, max, visibilityTimeout, longPollTimeout);

    public Task AcknowledgeMessageAsync(string queueName, string receiptHandle) =>
        _repository.AcknowledgeMessageAsync(queueName, receiptHandle);

    public Task RejectMessageAsync(string queueName, string receiptHandle) =>
        _repository.RejectMessageAsync(queueName, receiptHandle);

    public Task<QueueMessage?> PeekMessageAsync(string queueName) => _repository.PeekMessageAsync(queueName);

    public Task<IEnumerable<DeadLetterMessage>> GetDeadLetterMessagesAsync(string queueName) =>
        _repository.GetDeadLetterMessagesAsync(queueName);

    public Task RedriveDeadLetterMessageAsync(string queueName, string messageId) =>
        _repository.RedriveDeadLetterMessageAsync(queueName, messageId);

    public Task<bool> DeleteDeadLetterMessageAsync(string queueName, string messageId) =>
        _repository.DeleteDeadLetterMessageAsync(queueName, messageId);

    public Task<QueueStats> GetQueueStatisticsAsync(string queueName) =>
        _repository.GetQueueStatisticsAsync(queueName);

    public Task PurgeQueueAsync(string queueName) =>
        _repository.PurgeQueueAsync(queueName);

    public Task PauseQueueAsync(string queueName) =>
        _repository.PauseQueueAsync(queueName);

    public Task ResumeQueueAsync(string queueName) =>
        _repository.ResumeQueueAsync(queueName);

    public Task<QueueDefinition?> GetQueueConfigurationAsync(string queueName) =>
        _repository.GetQueueConfigurationAsync(queueName);

    public Task UpdateQueueConfigurationAsync(string queueName, QueueDefinition config) =>
        _repository.UpdateQueueConfigurationAsync(queueName, config);

    public Task ExtendVisibilityAsync(string queueName, string receiptHandle, TimeSpan visibilityTimeout) =>
        _repository.ExtendVisibilityAsync(queueName, receiptHandle, visibilityTimeout);
}
