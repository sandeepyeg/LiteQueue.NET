namespace LiteQueue.Domain.Enums;

public enum DeliveryStatus
{
    Pending,
    Delivered,
    Acknowledged,
    Rejected,
    DeadLettered,
    Expired
}
