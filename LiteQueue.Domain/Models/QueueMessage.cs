namespace LiteQueue.Domain.Models;

public class QueueMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int DeliveryCount { get; set; }
    public DateTimeOffset? VisibleUntil { get; set; }
    public DateTimeOffset? ExpireAt { get; set; }
}
