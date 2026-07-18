using System.Text.Json;
using LiteQueue.Domain.Interfaces;
using LiteQueue.Domain.Models;
using LiteQueue.Infrastructure.Utils;
using StackExchange.Redis;

namespace LiteQueue.Infrastructure.Redis;

public class RedisSchedulerRepository : ISchedulerRepository
{
    private readonly IDatabase _db;

    public RedisSchedulerRepository(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task ScheduleMessageAsync(string queueName, string body, DateTimeOffset executeAt)
    {
        var job = new ScheduledJob
        {
            Id = Guid.NewGuid().ToString(),
            QueueName = queueName,
            Body = body,
            ExecuteAt = executeAt,
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var serialized = JsonSerializer.Serialize(job);
        await _db.SortedSetAddAsync(RedisKeyBuilder.SchedulePendingKey(), serialized, executeAt.ToUnixTimeSeconds());
    }

    public async Task<IEnumerable<ScheduledJob>> GetPendingSchedulesAsync()
    {
        var values = await _db.SortedSetRangeByScoreAsync(RedisKeyBuilder.SchedulePendingKey());
        return values
            .Select(v => JsonSerializer.Deserialize<ScheduledJob>(v.ToString()))
            .Where(j => j != null)
            .Select(j => j!);
    }

    public async Task CreateRecurringJobAsync(RecurringJob job)
    {
        var serialized = JsonSerializer.Serialize(job);
        await _db.HashSetAsync(RedisKeyBuilder.RecurringJobKey(job.Id), new HashEntry[]
        {
            new("id", job.Id),
            new("data", serialized)
        });
        await _db.SetAddAsync(RedisKeyBuilder.RecurringJobsKey(), job.Id);
    }

    public async Task<IEnumerable<RecurringJob>> GetAllRecurringJobsAsync()
    {
        var members = await _db.SetMembersAsync(RedisKeyBuilder.RecurringJobsKey());
        var jobs = new List<RecurringJob>();

        foreach (var member in members)
        {
            var job = await GetRecurringJobAsync(member.ToString());
            if (job != null)
                jobs.Add(job);
        }

        return jobs;
    }

    public async Task UpdateRecurringJobAsync(RecurringJob job)
    {
        var serialized = JsonSerializer.Serialize(job);
        await _db.HashSetAsync(RedisKeyBuilder.RecurringJobKey(job.Id), "data", serialized);
    }

    public async Task DeleteRecurringJobAsync(string jobId)
    {
        await _db.KeyDeleteAsync(RedisKeyBuilder.RecurringJobKey(jobId));
        await _db.SetRemoveAsync(RedisKeyBuilder.RecurringJobsKey(), jobId);
    }

    public async Task<RecurringJob?> GetRecurringJobAsync(string jobId)
    {
        var data = await _db.HashGetAsync(RedisKeyBuilder.RecurringJobKey(jobId), "data");
        if (data.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<RecurringJob>(data.ToString());
    }

    public async Task DispatchScheduledMessagesAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var pendingKey = RedisKeyBuilder.SchedulePendingKey();

        var dueItems = await _db.SortedSetRangeByScoreAsync(pendingKey, double.NegativeInfinity, now);

        foreach (var item in dueItems)
        {
            var removed = await _db.SortedSetRemoveAsync(pendingKey, item);
            if (!removed) continue;

            var job = JsonSerializer.Deserialize<ScheduledJob>(item.ToString());
            if (job == null) continue;

            var messageId = Guid.NewGuid().ToString();
            var message = new QueueMessage
            {
                Id = messageId,
                Body = job.Body,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var messageJson = JsonSerializer.Serialize(message);
            await _db.StringSetAsync(RedisKeyBuilder.MessageKey(messageId), messageJson);
            await _db.ListLeftPushAsync(RedisKeyBuilder.QueueReadyKey(job.QueueName), messageId);
        }
    }

    public async Task UpdateNextExecutionAsync(string jobId, DateTimeOffset nextExecution)
    {
        var job = await GetRecurringJobAsync(jobId);
        if (job == null) return;

        job.NextExecution = nextExecution;
        await UpdateRecurringJobAsync(job);
    }
}
