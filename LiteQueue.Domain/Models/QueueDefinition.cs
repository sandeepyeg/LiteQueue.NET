namespace LiteQueue.Domain.Models;

public class QueueDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int MaxDeliveryAttempts { get; set; } = 5;
    public int VisibilityTimeoutSeconds { get; set; } = 60;
    public int MessageRetentionSeconds { get; set; } = 259200;
    public int DeadLetterRetentionSeconds { get; set; } = 1209600;
    public int MaxMessageSizeBytes { get; set; } = 262144;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
