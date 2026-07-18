namespace LiteQueue.Domain.Enums;

public enum MessageStatus
{
    Ready,
    InFlight,
    Delayed,
    DeadLettered,
    Acknowledged
}
