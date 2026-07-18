using LiteQueue.Domain.Enums;

namespace LiteQueue.Domain.Services;

public static class RetryCalculator
{
    public static TimeSpan CalculateDelay(RetryStrategy strategy, int attempt, int baseDelaySeconds)
    {
        return strategy switch
        {
            RetryStrategy.Immediate => TimeSpan.Zero,
            RetryStrategy.FixedDelay => TimeSpan.FromSeconds(baseDelaySeconds),
            RetryStrategy.Linear => TimeSpan.FromSeconds(attempt * baseDelaySeconds),
            RetryStrategy.Exponential => TimeSpan.FromSeconds(baseDelaySeconds * Math.Pow(2, attempt - 1)),
            RetryStrategy.ExponentialWithJitter => CalculateExponentialWithJitter(attempt, baseDelaySeconds),
            _ => TimeSpan.Zero
        };
    }

    private static TimeSpan CalculateExponentialWithJitter(int attempt, int baseDelaySeconds)
    {
        var exponentialDelay = baseDelaySeconds * Math.Pow(2, attempt - 1);
        var jitter = Random.Shared.NextDouble() * 0.5 * exponentialDelay;
        return TimeSpan.FromSeconds(exponentialDelay + jitter);
    }
}
