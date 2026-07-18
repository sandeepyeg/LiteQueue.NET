using System.Text.Json.Serialization;

namespace LiteQueue.Contracts.Topics;

public class CreateTopicRequest
{
    [JsonPropertyName("topicName")]
    public string TopicName { get; set; } = string.Empty;
}

public class CreateSubscriptionRequest
{
    [JsonPropertyName("subscriptionName")]
    public string SubscriptionName { get; set; } = string.Empty;

    [JsonPropertyName("messageFilter")]
    public Dictionary<string, string>? MessageFilter { get; set; }
}

public class PublishMessageRequest
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}
