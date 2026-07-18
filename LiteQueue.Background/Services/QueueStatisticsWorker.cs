using LiteQueue.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiteQueue.Background.Services;

public class QueueStatisticsWorker : BackgroundService
{
    private readonly IQueueRepository _queueRepository;
    private readonly ILogger<QueueStatisticsWorker> _logger;

    public QueueStatisticsWorker(
        IQueueRepository queueRepository,
        ILogger<QueueStatisticsWorker> logger)
    {
        _queueRepository = queueRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QueueStatisticsWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var queues = await _queueRepository.ListQueuesAsync();

                foreach (var queueName in queues)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var stats = await _queueRepository.GetQueueStatisticsAsync(queueName);
                    _logger.LogInformation(
                        "Queue statistics for {QueueName}: Ready={ReadyCount}, InFlight={InFlightCount}, Delayed={DelayedCount}, DLQ={DeadLetterCount}.",
                        queueName, stats.ReadyCount, stats.InFlightCount, stats.DelayedCount, stats.DeadLetterCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting queue statistics.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("QueueStatisticsWorker stopped.");
    }
}
