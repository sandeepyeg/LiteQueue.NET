namespace LiteQueue.Domain.Models;

public class TopicDefinition
{
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
