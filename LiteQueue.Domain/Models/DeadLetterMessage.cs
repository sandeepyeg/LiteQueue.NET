using LiteQueue.Domain.Enums;

namespace LiteQueue.Domain.Models;

public class DeadLetterMessage
{
    public string Id { get; set; } = string.Empty;
    public string OriginalQueueName { get; set; } = string.Empty;
    public string OriginalMessageId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int DeliveryAttempts { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset DeadLetteredAt { get; set; } = DateTimeOffset.UtcNow;
    public DeadLetterReason FailureReason { get; set; }
    public string? ExceptionSummary { get; set; }
    public DateTimeOffset FirstFailedAt { get; set; }
    public DateTimeOffset LastFailedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
