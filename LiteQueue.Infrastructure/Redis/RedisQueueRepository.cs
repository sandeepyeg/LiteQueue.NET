namespace DefaultNamespace;

public class RedisQueueRepository : IQueueRepository
{
    private readonly IDatabase _db;

    public RedisQueueRepository(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public Task CreateQueueAsync(string queueName)
    {
        // Redis doesn't need to "create" a queue. You can no-op or validate.
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<string>> ListQueuesAsync()
    {
        // Redis has no native way to list keys unless using scan
        return Enumerable.Empty<string>(); // Placeholder
    }

    public async Task SendMessageAsync(string queueName, QueueMessageDto message)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(message);
        await _db.ListLeftPushAsync(queueName, json);
    }
    public async Task<IEnumerable<QueueMessageDto>> ReceiveMessagesAsync(string queueName, int maxMessages, TimeSpan visibilityTimeout)
{
    var messages = new List<QueueMessageDto>();

    for (int i = 0; i < maxMessages; i++)
    {
        var value = await _db.ListRightPopAsync(RedisKeyBuilder.QueueKey(queueName));
        if (value.IsNullOrEmpty) break;

        var msg = JsonSerializer.Deserialize<QueueMessageDto>(value!)!;
        msg.VisibleUntil = DateTimeOffset.UtcNow.Add(visibilityTimeout);
        msg.RetryCount += 1;

        if (msg.RetryCount > 5)
        {
            // Dead-letter it
            await _db.ListLeftPushAsync(RedisKeyBuilder.DeadLetterKey(queueName), JsonSerializer.Serialize(msg));
            continue;
        }

        messages.Add(msg);
    }

    return messages;
}


    public Task DeleteMessageAsync(string queueName, string messageId)
    {
        // Messages are already removed on receive — so this can be a no-op unless using visibility queues.
        return Task.CompletedTask;
    }

    public Task<QueueMessageDto?> PeekMessageAsync(string queueName)
    {
        // Redis has no "peek" — you'd have to use ListGetByIndex (index 0 or -1)
        return Task.FromResult<QueueMessageDto?>(null);
    }
}