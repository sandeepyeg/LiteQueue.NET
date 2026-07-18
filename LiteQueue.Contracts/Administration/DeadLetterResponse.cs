using System.Text.Json.Serialization;

namespace LiteQueue.Contracts.Administration;

public class DeadLetterResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("originalQueueName")]
    public string OriginalQueueName { get; set; } = string.Empty;

    [JsonPropertyName("originalMessageId")]
    public string OriginalMessageId { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("deliveryAttempts")]
    public int DeliveryAttempts { get; set; }

    [JsonPropertyName("lastError")]
    public string? LastError { get; set; }

    [JsonPropertyName("deadLetteredAt")]
    public DateTimeOffset DeadLetteredAt { get; set; }
}
