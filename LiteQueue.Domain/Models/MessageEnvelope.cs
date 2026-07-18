using LiteQueue.Domain.Enums;

namespace LiteQueue.Domain.Models;

public class MessageEnvelope
{
    public string MessageId { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? CorrelationId { get; set; }
    public string? IdempotencyKey { get; set; }
    public int Priority { get; set; }
    public int DeliveryCount { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FirstReceivedAt { get; set; }
    public DateTimeOffset? LastReceivedAt { get; set; }
    public DateTimeOffset? LastFailedAt { get; set; }
    public DateTimeOffset? AvailableAt { get; set; }
    public DateTimeOffset? ExpireAt { get; set; }
}
