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

    public Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(string queueName, int max, TimeSpan timeout) =>
        _repository.ReceiveMessagesAsync(queueName, max, timeout);

    public Task AcknowledgeMessageAsync(string queueName, string receiptHandle) =>
        _repository.AcknowledgeMessageAsync(queueName, receiptHandle);

    public Task RejectMessageAsync(string queueName, string receiptHandle) =>
        _repository.RejectMessageAsync(queueName, receiptHandle);

    public Task<QueueMessage?> PeekMessageAsync(string queueName) => _repository.PeekMessageAsync(queueName);

    public Task<IEnumerable<DeadLetterMessage>> GetDeadLetterMessagesAsync(string queueName) =>
        _repository.GetDeadLetterMessagesAsync(queueName);

    public Task RedriveDeadLetterMessageAsync(string queueName, string messageId) =>
        _repository.RedriveDeadLetterMessageAsync(queueName, messageId);
}
