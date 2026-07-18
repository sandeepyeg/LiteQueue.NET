namespace LiteQueue.Domain.Models;

public class RecurringJob
{
    public string Id { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    public string Body { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public DateTimeOffset? NextExecution { get; set; }
    public DateTimeOffset? LastExecution { get; set; }
    public string MisfirePolicy { get; set; } = "Skip";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
