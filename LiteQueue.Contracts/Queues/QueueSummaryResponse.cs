using System.Text.Json.Serialization;

namespace LiteQueue.Contracts.Queues;

public class QueueSummaryResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Active";

    [JsonPropertyName("readyCount")]
    public long ReadyCount { get; set; }

    [JsonPropertyName("inFlightCount")]
    public long InFlightCount { get; set; }

    [JsonPropertyName("delayedCount")]
    public long DelayedCount { get; set; }

    [JsonPropertyName("deadLetterCount")]
    public long DeadLetterCount { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }
}
