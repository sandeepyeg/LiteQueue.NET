using System.Text.Json.Serialization;

namespace LiteQueue.Contracts.Queues;

public class CreateQueueRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("maxDeliveryAttempts")]
    public int? MaxDeliveryAttempts { get; set; }

    [JsonPropertyName("visibilityTimeoutSeconds")]
    public int? VisibilityTimeoutSeconds { get; set; }

    [JsonPropertyName("messageRetentionSeconds")]
    public int? MessageRetentionSeconds { get; set; }

    [JsonPropertyName("deadLetterRetentionSeconds")]
    public int? DeadLetterRetentionSeconds { get; set; }

    [JsonPropertyName("maxMessageSizeBytes")]
    public int? MaxMessageSizeBytes { get; set; }
}
