using LiteQueue.Domain.Enums;

namespace LiteQueue.Domain.Models;

public class MessageFailure
{
    public string MessageId { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public DeadLetterReason Reason { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExceptionSummary { get; set; }
    public DateTimeOffset FailedAt { get; set; } = DateTimeOffset.UtcNow;
    public int DeliveryAttempt { get; set; }
}
