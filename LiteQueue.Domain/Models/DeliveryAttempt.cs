namespace LiteQueue.Domain.Models;

public class DeliveryAttempt
{
    public string MessageId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
}
