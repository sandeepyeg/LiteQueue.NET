using System.Text.Json;
using LiteQueue.Domain.Interfaces;
using LiteQueue.Infrastructure.Utils;
using StackExchange.Redis;
using QueueMessage = LiteQueue.Domain.Models.QueueMessage;
using DeadLetterMessage = LiteQueue.Domain.Models.DeadLetterMessage;

namespace LiteQueue.Infrastructure.Redis;

public class RedisQueueRepository : IQueueRepository
{
    private readonly IDatabase _db;

    public RedisQueueRepository(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public Task CreateQueueAsync(string queueName)
    {
        return _db.SetAddAsync(RedisKeyBuilder.QueuesRegistryKey(), queueName);
    }

    public async Task<IEnumerable<string>> ListQueuesAsync()
    {
        var values = await _db.SetMembersAsync(RedisKeyBuilder.QueuesRegistryKey());
        return values.Select(v => v.ToString());
    }

    public async Task SendMessageAsync(string queueName, QueueMessage message)
    {
        await CreateQueueAsync(queueName);

        var json = JsonSerializer.Serialize(message);
        await _db.StringSetAsync(RedisKeyBuilder.MessageKey(message.Id), json);

        var queueKey = RedisKeyBuilder.QueueReadyKey(queueName);
        await _db.ListLeftPushAsync(queueKey, message.Id);
    }

    public async Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(string queueName, int maxMessages, TimeSpan visibilityTimeout)
    {
        var messages = new List<QueueMessage>();

        for (int i = 0; i < maxMessages; i++)
        {
            var value = await _db.ListRightPopAsync(RedisKeyBuilder.QueueReadyKey(queueName));
            if (value.IsNullOrEmpty) break;

            var msg = await _db.StringGetAsync(RedisKeyBuilder.MessageKey(value!));
            if (msg.IsNullOrEmpty) continue;

            var queueMessage = JsonSerializer.Deserialize<QueueMessage>(msg!.ToString())!;
            queueMessage.DeliveryCount++;
            queueMessage.VisibleUntil = DateTimeOffset.UtcNow.Add(visibilityTimeout);

            if (queueMessage.DeliveryCount > 5)
            {
                var deadMsg = new DeadLetterMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    OriginalQueueName = queueName,
                    OriginalMessageId = queueMessage.Id,
                    Body = queueMessage.Body,
                    DeliveryAttempts = queueMessage.DeliveryCount,
                    DeadLetteredAt = DateTimeOffset.UtcNow
                };
                await _db.ListLeftPushAsync(RedisKeyBuilder.DeadLetterKey(queueName), JsonSerializer.Serialize(deadMsg));
                continue;
            }

            var receiptHandle = Guid.NewGuid().ToString();
            await _db.SortedSetAddAsync(RedisKeyBuilder.QueueInFlightKey(queueName), value!, DateTimeOffset.UtcNow.Add(visibilityTimeout).ToUnixTimeSeconds());
            await _db.StringSetAsync(RedisKeyBuilder.ReceiptKey(receiptHandle), JsonSerializer.Serialize(value!));
            await _db.StringSetAsync(RedisKeyBuilder.MessageKey(queueMessage.Id), JsonSerializer.Serialize(queueMessage));

            messages.Add(queueMessage);
        }

        return messages;
    }

    public Task AcknowledgeMessageAsync(string queueName, string receiptHandle)
    {
        return Task.CompletedTask;
    }

    public Task RejectMessageAsync(string queueName, string receiptHandle)
    {
        return Task.CompletedTask;
    }

    public async Task<QueueMessage?> PeekMessageAsync(string queueName)
    {
        var value = await _db.ListGetByIndexAsync(RedisKeyBuilder.QueueReadyKey(queueName), -1);
        if (value.IsNullOrEmpty) return null;

        var msg = await _db.StringGetAsync(RedisKeyBuilder.MessageKey(value!));
        if (msg.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<QueueMessage>(msg!.ToString());
    }

    public async Task<IEnumerable<DeadLetterMessage>> GetDeadLetterMessagesAsync(string queueName)
    {
        var values = await _db.ListRangeAsync(RedisKeyBuilder.DeadLetterKey(queueName));
        return values
            .Select(v => JsonSerializer.Deserialize<DeadLetterMessage>(v!.ToString()))
            .Where(m => m != null)
            .Select(m => m!);
    }

    public Task RedriveDeadLetterMessageAsync(string queueName, string messageId)
    {
        return Task.CompletedTask;
    }
}
