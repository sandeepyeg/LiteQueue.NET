using LiteQueue.Domain.Interfaces;
using LiteQueue.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiteQueue.Background.Services;

public class RecurringJobScheduler : BackgroundService
{
    private readonly ISchedulerRepository _schedulerRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly ILogger<RecurringJobScheduler> _logger;

    public RecurringJobScheduler(
        ISchedulerRepository schedulerRepository,
        IQueueRepository queueRepository,
        ILogger<RecurringJobScheduler> logger)
    {
        _schedulerRepository = schedulerRepository;
        _queueRepository = queueRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RecurringJobScheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var jobs = await _schedulerRepository.GetAllRecurringJobsAsync();
                var now = DateTimeOffset.UtcNow;

                foreach (var job in jobs)
                {
                    if (!job.Enabled) continue;
                    if (job.NextExecution == null || job.NextExecution > now) continue;

                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(job.TimeZone);

                    var message = new Domain.Models.QueueMessage
                    {
                        Id = Guid.NewGuid().ToString(),
                        Body = job.Body,
                        CreatedAt = now
                    };

                    await _queueRepository.SendMessageAsync(job.QueueName, message);

                    var next = CronParser.GetNextOccurrence(job.CronExpression, now, timeZone);
                    if (next != null)
                    {
                        await _schedulerRepository.UpdateNextExecutionAsync(job.Id, next.Value);
                    }

                    _logger.LogInformation(
                        "Executed recurring job {JobId} for queue {QueueName}. Next execution at {NextExecution}.",
                        job.Id, job.QueueName, next);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring jobs.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _logger.LogInformation("RecurringJobScheduler stopped.");
    }
}
