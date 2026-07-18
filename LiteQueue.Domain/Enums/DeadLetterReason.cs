namespace LiteQueue.Domain.Enums;

public enum DeadLetterReason
{
    MaxAttemptsExceeded,
    MessageExpired,
    DeserializationFailed,
    InvalidPayload,
    ExplicitlyRejected
}
