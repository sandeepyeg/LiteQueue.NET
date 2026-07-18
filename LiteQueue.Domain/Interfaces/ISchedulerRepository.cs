using LiteQueue.Domain.Models;

namespace LiteQueue.Domain.Interfaces;

public interface ISchedulerRepository
{
    Task ScheduleMessageAsync(string queueName, string body, DateTimeOffset executeAt);
    Task<IEnumerable<ScheduledJob>> GetPendingSchedulesAsync();
    Task CreateRecurringJobAsync(RecurringJob job);
    Task<IEnumerable<RecurringJob>> GetAllRecurringJobsAsync();
    Task UpdateRecurringJobAsync(RecurringJob job);
    Task DeleteRecurringJobAsync(string jobId);
    Task<RecurringJob?> GetRecurringJobAsync(string jobId);
    Task DispatchScheduledMessagesAsync();
    Task UpdateNextExecutionAsync(string jobId, DateTimeOffset nextExecution);
}
