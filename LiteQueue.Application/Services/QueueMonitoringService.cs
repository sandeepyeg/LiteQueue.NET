using LiteQueue.Contracts.Queues;
using LiteQueue.Domain.Interfaces;

namespace LiteQueue.Application.Services;

public class QueueMonitoringService
{
    private readonly IQueueRepository _repository;

    public QueueMonitoringService(IQueueRepository repository)
    {
        _repository = repository;
    }

    public async Task<QueueSummaryResponse> GetQueueStatisticsAsync(string queueName)
    {
        var stats = await _repository.GetQueueStatisticsAsync(queueName);
        var config = await _repository.GetQueueConfigurationAsync(queueName);

        return new QueueSummaryResponse
        {
            Name = queueName,
            Status = config?.Status ?? "Active",
            ReadyCount = stats.ReadyCount,
            InFlightCount = stats.InFlightCount,
            DelayedCount = stats.DelayedCount,
            DeadLetterCount = stats.DeadLetterCount,
            CreatedAt = config?.CreatedAt ?? DateTimeOffset.UtcNow
        };
    }

    public async Task<IEnumerable<QueueSummaryResponse>> GetAllQueuesStatisticsAsync()
    {
        var queues = await _repository.ListQueuesAsync();
        var tasks = queues.Select(GetQueueStatisticsAsync);
        return await Task.WhenAll(tasks);
    }
}
