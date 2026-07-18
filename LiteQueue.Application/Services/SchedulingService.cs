using LiteQueue.Domain.Interfaces;
using LiteQueue.Domain.Models;

namespace LiteQueue.Application.Services;

public class SchedulingService
{
    private readonly ISchedulerRepository _repository;

    public SchedulingService(ISchedulerRepository repository)
    {
        _repository = repository;
    }

    public async Task ScheduleMessageAsync(string queueName, string body, DateTimeOffset executeAt)
    {
        if (executeAt <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Scheduled execution time must be in the future.", nameof(executeAt));

        await _repository.ScheduleMessageAsync(queueName, body, executeAt);
    }

    public Task<IEnumerable<ScheduledJob>> GetPendingSchedulesAsync() =>
        _repository.GetPendingSchedulesAsync();

    public async Task CreateRecurringJobAsync(RecurringJob job)
    {
        if (string.IsNullOrWhiteSpace(job.CronExpression))
            throw new ArgumentException("Cron expression is required.", nameof(job.CronExpression));

        await _repository.CreateRecurringJobAsync(job);
    }

    public Task<IEnumerable<RecurringJob>> GetAllRecurringJobsAsync() =>
        _repository.GetAllRecurringJobsAsync();

    public Task UpdateRecurringJobAsync(RecurringJob job) =>
        _repository.UpdateRecurringJobAsync(job);

    public Task DeleteRecurringJobAsync(string jobId) =>
        _repository.DeleteRecurringJobAsync(jobId);

    public Task<RecurringJob?> GetRecurringJobAsync(string jobId) =>
        _repository.GetRecurringJobAsync(jobId);

    public Task DispatchScheduledMessagesAsync() =>
        _repository.DispatchScheduledMessagesAsync();
}
