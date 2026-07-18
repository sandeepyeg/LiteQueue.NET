namespace LiteQueue.Domain.Models;

public class TopicSubscription
{
    public string Id { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, string>? MessageFilter { get; set; }
}
