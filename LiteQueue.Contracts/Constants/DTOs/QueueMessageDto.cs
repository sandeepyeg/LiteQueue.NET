namespace DefaultNamespace;

public class QueueMessageDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int RetryCount { get; set; } = 0;
    public DateTimeOffset? VisibleUntil { get; set; }
}