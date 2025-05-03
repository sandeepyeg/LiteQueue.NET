namespace DefaultNamespace;

public class QueueService
{
    private readonly IQueueRepository _repository;

    public QueueService(IQueueRepository repository)
    {
        _repository = repository;
    }
    public Task CreateQueueAsync(string queueName) => _repository.CreateQueueAsync(queueName);
    public Task<IEnumerable<string>> ListQueuesAsync() => _repository.ListQueuesAsync();
    public Task SendMessageAsync(string queueName, QueueMessageDto message) => _repository.SendMessageAsync(queueName, message);
    public Task<IEnumerable<QueueMessageDto>> ReceiveMessagesAsync(string queueName, int max, TimeSpan timeout) =>
        _repository.ReceiveMessagesAsync(queueName, max, timeout);
    public Task DeleteMessageAsync(string queueName, string messageId) => _repository.DeleteMessageAsync(queueName, messageId);
    public Task<QueueMessageDto?> PeekMessageAsync(string queueName) => _repository.PeekMessageAsync(queueName);
}