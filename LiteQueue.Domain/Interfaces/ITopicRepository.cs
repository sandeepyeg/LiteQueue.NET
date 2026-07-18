using LiteQueue.Domain.Models;

namespace LiteQueue.Domain.Interfaces;

public interface ITopicRepository
{
    Task CreateTopicAsync(string topicName);
    Task DeleteTopicAsync(string topicName);
    Task<IEnumerable<string>> ListTopicsAsync();
    Task CreateSubscriptionAsync(string topicName, string subscriptionName);
    Task DeleteSubscriptionAsync(string topicName, string subscriptionName);
    Task<IEnumerable<string>> ListSubscriptionsAsync(string topicName);
    Task PublishAsync(string topicName, QueueMessage message);
    Task<TopicDefinition?> GetTopicAsync(string topicName);
}
