using System.Text.Json;
using LiteQueue.Domain.Interfaces;
using LiteQueue.Infrastructure.Utils;
using StackExchange.Redis;
using QueueMessage = LiteQueue.Domain.Models.QueueMessage;
using DeadLetterMessage = LiteQueue.Domain.Models.DeadLetterMessage;
using QueueDefinition = LiteQueue.Domain.Models.QueueDefinition;

namespace LiteQueue.Infrastructure.Redis;

public class RedisQueueRepository : IQueueRepository
{
    private readonly IDatabase _db;
    private readonly IConnectionMultiplexer _redis;
    private IServer? _server;

    public RedisQueueRepository(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    private IServer GetServer()
    {
        if (_server == null)
            _server = _redis.GetServer(_redis.GetEndPoints().First());
        return _server;
    }

    public async Task CreateQueueAsync(string queueName)
    {
        await _db.SetAddAsync(RedisKeyBuilder.QueuesRegistryKey(), queueName);
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
        await _db.ListLeftPushAsync(RedisKeyBuilder.QueueReadyKey(queueName), message.Id);
    }

    public async Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(string queueName, int maxMessages, TimeSpan visibilityTimeout)
    {
        var messages = new List<QueueMessage>();
        var visibilityScore = DateTimeOffset.UtcNow.Add(visibilityTimeout).ToUnixTimeSeconds();

        for (int i = 0; i < maxMessages; i++)
        {
            var messageId = await _db.ListRightPopAsync(RedisKeyBuilder.QueueReadyKey(queueName));
            if (messageId.IsNullOrEmpty) break;

            var msgJson = await _db.StringGetAsync(RedisKeyBuilder.MessageKey(messageId.ToString()));
            if (msgJson.IsNullOrEmpty) continue;

            var queueMessage = JsonSerializer.Deserialize<QueueMessage>(msgJson.ToString())!;
            queueMessage.DeliveryCount++;
            queueMessage.VisibleUntil = DateTimeOffset.UtcNow.Add(visibilityTimeout);

            var receiptHandle = Guid.NewGuid().ToString();

            var config = await GetQueueConfigurationAsync(queueName);
            var maxAttempts = config?.MaxDeliveryAttempts ?? 5;

            if (queueMessage.DeliveryCount > maxAttempts)
            {
                var deadMsg = new DeadLetterMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    OriginalQueueName = queueName,
                    OriginalMessageId = messageId.ToString(),
                    Body = queueMessage.Body,
                    DeliveryAttempts = queueMessage.DeliveryCount,
                    DeadLetteredAt = DateTimeOffset.UtcNow,
                    FailureReason = Domain.Enums.DeadLetterReason.MaxAttemptsExceeded,
                    FirstFailedAt = queueMessage.CreatedAt,
                    LastFailedAt = DateTimeOffset.UtcNow
                };
                await _db.ListLeftPushAsync(RedisKeyBuilder.DeadLetterKey(queueName), JsonSerializer.Serialize(deadMsg));
                await _db.KeyDeleteAsync(RedisKeyBuilder.MessageKey(messageId.ToString()));
                continue;
            }

            await _db.SortedSetAddAsync(RedisKeyBuilder.QueueInFlightKey(queueName), messageId.ToString(), visibilityScore);
            await _db.StringSetAsync(RedisKeyBuilder.ReceiptKey(receiptHandle), messageId.ToString(), TimeSpan.FromSeconds(visibilityTimeout.TotalSeconds + 300));
            await _db.StringSetAsync(RedisKeyBuilder.MessageKey(messageId.ToString()), JsonSerializer.Serialize(queueMessage));

            messages.Add(new QueueMessage
            {
                Id = messageId.ToString(),
                Body = queueMessage.Body,
                CreatedAt = queueMessage.CreatedAt,
                DeliveryCount = queueMessage.DeliveryCount,
                VisibleUntil = queueMessage.VisibleUntil
            });
        }

        return messages;
    }

    public async Task<IEnumerable<QueueMessage>> ReceiveWithLongPollingAsync(string queueName, int maxMessages, TimeSpan visibilityTimeout, TimeSpan longPollTimeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(longPollTimeout);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var messages = (await ReceiveMessagesAsync(queueName, maxMessages, visibilityTimeout)).ToList();
            if (messages.Count > 0) return messages;
            if (DateTimeOffset.UtcNow >= deadline) break;
            await Task.Delay(200);
        }

        return Enumerable.Empty<QueueMessage>();
    }

    public async Task AcknowledgeMessageAsync(string queueName, string receiptHandle)
    {
        var receiptKey = RedisKeyBuilder.ReceiptKey(receiptHandle);
        var messageIdValue = await _db.StringGetAsync(receiptKey);
        if (messageIdValue.IsNullOrEmpty) return;

        var messageId = messageIdValue.ToString();
        await _db.SortedSetRemoveAsync(RedisKeyBuilder.QueueInFlightKey(queueName), messageId);
        await _db.KeyDeleteAsync(receiptKey);
        await _db.KeyDeleteAsync(RedisKeyBuilder.MessageKey(messageId));
    }

    public async Task RejectMessageAsync(string queueName, string receiptHandle)
    {
        var receiptKey = RedisKeyBuilder.ReceiptKey(receiptHandle);
        var messageIdValue = await _db.StringGetAsync(receiptKey);
        if (messageIdValue.IsNullOrEmpty) return;

        var messageId = messageIdValue.ToString();
        var messageKey = RedisKeyBuilder.MessageKey(messageId);
        var msgJson = await _db.StringGetAsync(messageKey);
        if (msgJson.IsNullOrEmpty) return;

        var message = JsonSerializer.Deserialize<QueueMessage>(msgJson.ToString());
        var deliveryCount = message?.DeliveryCount ?? 0;
        var config = await GetQueueConfigurationAsync(queueName);
        var maxAttempts = config?.MaxDeliveryAttempts ?? 5;

        await _db.SortedSetRemoveAsync(RedisKeyBuilder.QueueInFlightKey(queueName), messageId);
        await _db.KeyDeleteAsync(receiptKey);

        if (deliveryCount >= maxAttempts)
        {
            var deadMsg = new DeadLetterMessage
            {
                Id = Guid.NewGuid().ToString(),
                OriginalQueueName = queueName,
                OriginalMessageId = messageId,
                Body = message?.Body ?? string.Empty,
                DeliveryAttempts = deliveryCount,
                DeadLetteredAt = DateTimeOffset.UtcNow,
                FailureReason = Domain.Enums.DeadLetterReason.MaxAttemptsExceeded,
                FirstFailedAt = message?.CreatedAt ?? DateTimeOffset.UtcNow,
                LastFailedAt = DateTimeOffset.UtcNow
            };
            await _db.ListLeftPushAsync(RedisKeyBuilder.DeadLetterKey(queueName), JsonSerializer.Serialize(deadMsg));
            await _db.KeyDeleteAsync(messageKey);
        }
        else
        {
            await _db.ListLeftPushAsync(RedisKeyBuilder.QueueReadyKey(queueName), messageId);
        }
    }

    public async Task<QueueMessage?> PeekMessageAsync(string queueName)
    {
        var value = await _db.ListGetByIndexAsync(RedisKeyBuilder.QueueReadyKey(queueName), -1);
        if (value.IsNullOrEmpty) return null;

        var msg = await _db.StringGetAsync(RedisKeyBuilder.MessageKey(value.ToString()));
        if (msg.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<QueueMessage>(msg.ToString());
    }

    public async Task<IEnumerable<DeadLetterMessage>> GetDeadLetterMessagesAsync(string queueName)
    {
        var values = await _db.ListRangeAsync(RedisKeyBuilder.DeadLetterKey(queueName));
        return values
            .Select(v => JsonSerializer.Deserialize<DeadLetterMessage>(v!.ToString()))
            .Where(m => m != null)
            .Select(m => m!);
    }

    public async Task RedriveDeadLetterMessageAsync(string queueName, string messageId)
    {
        var deadKey = RedisKeyBuilder.DeadLetterKey(queueName);
        var deadMessages = await _db.ListRangeAsync(deadKey);

        foreach (var item in deadMessages)
        {
            var deadMsg = JsonSerializer.Deserialize<DeadLetterMessage>(item!.ToString());
            if (deadMsg?.Id == messageId || deadMsg?.OriginalMessageId == messageId)
            {
                var queueMessage = new QueueMessage
                {
                    Id = deadMsg.OriginalMessageId,
                    Body = deadMsg.Body,
                    CreatedAt = DateTimeOffset.UtcNow,
                    DeliveryCount = 0
                };
                await _db.StringSetAsync(RedisKeyBuilder.MessageKey(queueMessage.Id), JsonSerializer.Serialize(queueMessage));
                await _db.ListLeftPushAsync(RedisKeyBuilder.QueueReadyKey(queueName), queueMessage.Id);
                await _db.ListRemoveAsync(deadKey, item);
                return;
            }
        }
    }

    public async Task<bool> DeleteDeadLetterMessageAsync(string queueName, string messageId)
    {
        var deadKey = RedisKeyBuilder.DeadLetterKey(queueName);
        var deadMessages = await _db.ListRangeAsync(deadKey);

        foreach (var item in deadMessages)
        {
            var deadMsg = JsonSerializer.Deserialize<DeadLetterMessage>(item!.ToString());
            if (deadMsg?.Id == messageId || deadMsg?.OriginalMessageId == messageId)
            {
                await _db.ListRemoveAsync(deadKey, item);
                return true;
            }
        }
        return false;
    }

    public async Task<string?> GetDeduplicationMessageIdAsync(string queueName, string idempotencyKey)
    {
        var dedupeKey = RedisKeyBuilder.DedupeKey(queueName, idempotencyKey);
        var value = await _db.StringGetAsync(dedupeKey);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task StoreDeduplicationKeyAsync(string queueName, string idempotencyKey, string messageId)
    {
        var dedupeKey = RedisKeyBuilder.DedupeKey(queueName, idempotencyKey);
        await _db.StringSetAsync(dedupeKey, messageId, TimeSpan.FromHours(24));
    }

    public async Task SendMessageWithDelayAsync(string queueName, QueueMessage message, TimeSpan delay)
    {
        await CreateQueueAsync(queueName);
        var json = JsonSerializer.Serialize(message);
        await _db.StringSetAsync(RedisKeyBuilder.MessageKey(message.Id), json);

        if (delay > TimeSpan.Zero)
        {
            var score = DateTimeOffset.UtcNow.Add(delay).ToUnixTimeSeconds();
            await _db.SortedSetAddAsync(RedisKeyBuilder.QueueDelayedKey(queueName), message.Id, score);
        }
        else
        {
            await _db.ListLeftPushAsync(RedisKeyBuilder.QueueReadyKey(queueName), message.Id);
        }
    }

    public async Task<QueueStats> GetQueueStatisticsAsync(string queueName)
    {
        var readyCount = await _db.ListLengthAsync(RedisKeyBuilder.QueueReadyKey(queueName));
        var inflightCount = await _db.SortedSetLengthAsync(RedisKeyBuilder.QueueInFlightKey(queueName));
        var delayedCount = await _db.SortedSetLengthAsync(RedisKeyBuilder.QueueDelayedKey(queueName));
        var deadCount = await _db.ListLengthAsync(RedisKeyBuilder.DeadLetterKey(queueName));

        return new QueueStats
        {
            ReadyCount = readyCount,
            InFlightCount = inflightCount,
            DelayedCount = delayedCount,
            DeadLetterCount = deadCount
        };
    }

    public async Task PurgeQueueAsync(string queueName)
    {
        var keys = new RedisKey[]
        {
            RedisKeyBuilder.QueueReadyKey(queueName),
            RedisKeyBuilder.QueueInFlightKey(queueName),
            RedisKeyBuilder.QueueDelayedKey(queueName),
            RedisKeyBuilder.DeadLetterKey(queueName),
            RedisKeyBuilder.QueueConfigKey(queueName)
        };
        await _db.KeyDeleteAsync(keys);
    }

    public Task PauseQueueAsync(string queueName)
    {
        return _db.HashSetAsync(RedisKeyBuilder.QueueConfigKey(queueName), "status", "Paused");
    }

    public Task ResumeQueueAsync(string queueName)
    {
        return _db.HashSetAsync(RedisKeyBuilder.QueueConfigKey(queueName), "status", "Active");
    }

    public async Task<QueueDefinition?> GetQueueConfigurationAsync(string queueName)
    {
        var configKey = RedisKeyBuilder.QueueConfigKey(queueName);
        var exists = await _db.KeyExistsAsync(configKey);
        if (!exists) return null;

        var hash = await _db.HashGetAllAsync(configKey);
        var dict = new Dictionary<string, string>();
        foreach (var entry in hash)
            dict[entry.Name.ToString()] = entry.Value.ToString();

        return new QueueDefinition
        {
            Name = dict.GetValueOrDefault("name", queueName),
            Status = dict.GetValueOrDefault("status", "Active"),
            MaxDeliveryAttempts = int.TryParse(dict.GetValueOrDefault("maxDeliveryAttempts", "5"), out var mda) ? mda : 5,
            VisibilityTimeoutSeconds = int.TryParse(dict.GetValueOrDefault("visibilityTimeoutSeconds", "60"), out var vts) ? vts : 60,
            MessageRetentionSeconds = int.TryParse(dict.GetValueOrDefault("messageRetentionSeconds", "259200"), out var mrs) ? mrs : 259200,
            DeadLetterRetentionSeconds = int.TryParse(dict.GetValueOrDefault("deadLetterRetentionSeconds", "1209600"), out var dlrs) ? dlrs : 1209600,
            MaxMessageSizeBytes = int.TryParse(dict.GetValueOrDefault("maxMessageSizeBytes", "262144"), out var mms) ? mms : 262144,
            CreatedAt = DateTimeOffset.TryParse(dict.GetValueOrDefault("createdAt", ""), out var ca) ? ca : DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public async Task UpdateQueueConfigurationAsync(string queueName, QueueDefinition config)
    {
        var configKey = RedisKeyBuilder.QueueConfigKey(queueName);
        config.UpdatedAt = DateTimeOffset.UtcNow;

        var hashEntries = new HashEntry[]
        {
            new("name", config.Name),
            new("status", config.Status),
            new("maxDeliveryAttempts", config.MaxDeliveryAttempts.ToString()),
            new("visibilityTimeoutSeconds", config.VisibilityTimeoutSeconds.ToString()),
            new("messageRetentionSeconds", config.MessageRetentionSeconds.ToString()),
            new("deadLetterRetentionSeconds", config.DeadLetterRetentionSeconds.ToString()),
            new("maxMessageSizeBytes", config.MaxMessageSizeBytes.ToString()),
            new("createdAt", config.CreatedAt.ToString("O")),
            new("updatedAt", config.UpdatedAt.ToString("O"))
        };
        await _db.HashSetAsync(configKey, hashEntries);
    }

    public async Task ExtendVisibilityAsync(string queueName, string receiptHandle, TimeSpan visibilityTimeout)
    {
        var receiptKey = RedisKeyBuilder.ReceiptKey(receiptHandle);
        var messageIdValue = await _db.StringGetAsync(receiptKey);
        if (messageIdValue.IsNullOrEmpty) return;

        var newScore = DateTimeOffset.UtcNow.Add(visibilityTimeout).ToUnixTimeSeconds();
        await _db.SortedSetAddAsync(RedisKeyBuilder.QueueInFlightKey(queueName), messageIdValue.ToString(), newScore);
        await _db.KeyExpireAsync(receiptKey, visibilityTimeout.Add(TimeSpan.FromMinutes(5)));
    }
}
