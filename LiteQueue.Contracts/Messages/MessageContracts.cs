using System.Text.Json.Serialization;

namespace LiteQueue.Contracts.Messages;

public class EnqueueMessageRequest
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("delaySeconds")]
    public int? DelaySeconds { get; set; }

    [JsonPropertyName("availableAt")]
    public DateTimeOffset? AvailableAt { get; set; }

    [JsonPropertyName("idempotencyKey")]
    public string? IdempotencyKey { get; set; }
}

public class EnqueueMessageResponse
{
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;
}

public class ReceivedMessage
{
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("receiptHandle")]
    public string ReceiptHandle { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("deliveryCount")]
    public int DeliveryCount { get; set; }

    [JsonPropertyName("visibleUntil")]
    public DateTimeOffset VisibleUntil { get; set; }
}

public class AcknowledgeRequest
{
    [JsonPropertyName("receiptHandle")]
    public string ReceiptHandle { get; set; } = string.Empty;
}

public class RejectRequest
{
    [JsonPropertyName("receiptHandle")]
    public string ReceiptHandle { get; set; } = string.Empty;
}

public class ChangeVisibilityRequest
{
    [JsonPropertyName("receiptHandle")]
    public string ReceiptHandle { get; set; } = string.Empty;

    [JsonPropertyName("visibilityTimeoutSeconds")]
    public int VisibilityTimeoutSeconds { get; set; }
}
