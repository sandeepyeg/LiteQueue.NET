namespace LiteQueue.Domain.Models;

public class ScheduledJob
{
    public string Id { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset ExecuteAt { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
