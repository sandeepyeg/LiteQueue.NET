namespace LiteQueue.Domain.Models;

public class InFlightMessage
{
    public string MessageId { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string ReceiptHandle { get; set; } = string.Empty;
    public DateTimeOffset VisibleUntil { get; set; }
    public int DeliveryAttempt { get; set; }
}
