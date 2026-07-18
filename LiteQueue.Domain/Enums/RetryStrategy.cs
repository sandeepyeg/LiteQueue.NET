namespace LiteQueue.Domain.Enums;

public enum RetryStrategy
{
    Immediate,
    FixedDelay,
    Linear,
    Exponential,
    ExponentialWithJitter
}
