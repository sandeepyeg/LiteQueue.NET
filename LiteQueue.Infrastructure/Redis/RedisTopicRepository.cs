using System.Text.Json;
using LiteQueue.Domain.Interfaces;
using LiteQueue.Domain.Models;
using LiteQueue.Infrastructure.Utils;
using StackExchange.Redis;

namespace LiteQueue.Infrastructure.Redis;

public class RedisTopicRepository : ITopicRepository
{
    private readonly IDatabase _db;

    public RedisTopicRepository(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task CreateTopicAsync(string topicName)
    {
        await _db.SetAddAsync(RedisKeyBuilder.TopicsRegistryKey(), topicName);
    }

    public async Task DeleteTopicAsync(string topicName)
    {
        var subscriptions = await _db.SetMembersAsync(RedisKeyBuilder.TopicSubscriptionsKey(topicName));
        foreach (var sub in subscriptions)
        {
            await _db.KeyDeleteAsync(RedisKeyBuilder.SubscriptionQueueKey(topicName, sub.ToString()));
        }
        await _db.KeyDeleteAsync(RedisKeyBuilder.TopicSubscriptionsKey(topicName));
        await _db.SetRemoveAsync(RedisKeyBuilder.TopicsRegistryKey(), topicName);
    }

    public async Task<IEnumerable<string>> ListTopicsAsync()
    {
        var values = await _db.SetMembersAsync(RedisKeyBuilder.TopicsRegistryKey());
        return values.Select(v => v.ToString());
    }

    public async Task CreateSubscriptionAsync(string topicName, string subscriptionName)
    {
        await _db.SetAddAsync(RedisKeyBuilder.TopicSubscriptionsKey(topicName), subscriptionName);
        var topicExists = await _db.SetContainsAsync(RedisKeyBuilder.TopicsRegistryKey(), topicName);
        if (!topicExists)
        {
            await _db.SetAddAsync(RedisKeyBuilder.TopicsRegistryKey(), topicName);
        }
    }

    public async Task DeleteSubscriptionAsync(string topicName, string subscriptionName)
    {
        await _db.SetRemoveAsync(RedisKeyBuilder.TopicSubscriptionsKey(topicName), subscriptionName);
        await _db.KeyDeleteAsync(RedisKeyBuilder.SubscriptionQueueKey(topicName, subscriptionName));
    }

    public async Task<IEnumerable<string>> ListSubscriptionsAsync(string topicName)
    {
        var values = await _db.SetMembersAsync(RedisKeyBuilder.TopicSubscriptionsKey(topicName));
        return values.Select(v => v.ToString());
    }

    public async Task PublishAsync(string topicName, QueueMessage message)
    {
        var subscriptionNames = await _db.SetMembersAsync(RedisKeyBuilder.TopicSubscriptionsKey(topicName));
        foreach (var sub in subscriptionNames)
        {
            var json = JsonSerializer.Serialize(message);
            await _db.ListLeftPushAsync(RedisKeyBuilder.SubscriptionQueueKey(topicName, sub.ToString()), json);
        }
    }

    public async Task<TopicDefinition?> GetTopicAsync(string topicName)
    {
        var exists = await _db.SetContainsAsync(RedisKeyBuilder.TopicsRegistryKey(), topicName);
        if (!exists) return null;
        return new TopicDefinition { Name = topicName };
    }
}
