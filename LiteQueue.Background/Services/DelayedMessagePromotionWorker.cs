using LiteQueue.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiteQueue.Background.Services;

public class DelayedMessagePromotionWorker : BackgroundService
{
    private readonly ISchedulerRepository _schedulerRepository;
    private readonly ILogger<DelayedMessagePromotionWorker> _logger;

    public DelayedMessagePromotionWorker(
        ISchedulerRepository schedulerRepository,
        ILogger<DelayedMessagePromotionWorker> logger)
    {
        _schedulerRepository = schedulerRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DelayedMessagePromotionWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _schedulerRepository.DispatchScheduledMessagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching scheduled messages.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _logger.LogInformation("DelayedMessagePromotionWorker stopped.");
    }
}
