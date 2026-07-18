using LiteQueue.Domain.Enums;

namespace LiteQueue.Domain.Models;

public class QueueStatistics
{
    public string QueueName { get; set; } = string.Empty;
    public long ReadyCount { get; set; }
    public long InFlightCount { get; set; }
    public long DelayedCount { get; set; }
    public long DeadLetterCount { get; set; }
    public long AcknowledgedCount { get; set; }
    public long RejectedCount { get; set; }
    public long ExpiredCount { get; set; }
    public DateTimeOffset? OldestMessageAge { get; set; }
}
