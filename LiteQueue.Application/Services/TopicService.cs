using LiteQueue.Domain.Interfaces;
using LiteQueue.Domain.Models;

namespace LiteQueue.Application.Services;

public class TopicService
{
    private readonly ITopicRepository _repository;

    public TopicService(ITopicRepository repository)
    {
        _repository = repository;
    }

    public Task CreateTopicAsync(string topicName) => _repository.CreateTopicAsync(topicName);

    public Task DeleteTopicAsync(string topicName) => _repository.DeleteTopicAsync(topicName);

    public Task<IEnumerable<string>> ListTopicsAsync() => _repository.ListTopicsAsync();

    public Task CreateSubscriptionAsync(string topicName, string subscriptionName) =>
        _repository.CreateSubscriptionAsync(topicName, subscriptionName);

    public Task DeleteSubscriptionAsync(string topicName, string subscriptionName) =>
        _repository.DeleteSubscriptionAsync(topicName, subscriptionName);

    public Task<IEnumerable<string>> ListSubscriptionsAsync(string topicName) =>
        _repository.ListSubscriptionsAsync(topicName);

    public Task PublishAsync(string topicName, QueueMessage message) =>
        _repository.PublishAsync(topicName, message);

    public Task<TopicDefinition?> GetTopicAsync(string topicName) =>
        _repository.GetTopicAsync(topicName);
}
