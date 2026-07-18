using LiteQueue.Domain.Services;
using LiteQueue.Domain.Enums;

namespace LiteQueue.Tests.Domain;

public class RetryCalculatorTests
{
    [Fact]
    public void Immediate_ReturnsZero()
    {
        var delay = RetryCalculator.CalculateDelay(RetryStrategy.Immediate, 1, 5);
        Assert.Equal(TimeSpan.Zero, delay);
    }

    [Fact]
    public void FixedDelay_ReturnsBase()
    {
        var delay = RetryCalculator.CalculateDelay(RetryStrategy.FixedDelay, 3, 10);
        Assert.Equal(TimeSpan.FromSeconds(10), delay);
    }

    [Fact]
    public void Linear_IncreasesWithAttempt()
    {
        var delay1 = RetryCalculator.CalculateDelay(RetryStrategy.Linear, 1, 5);
        var delay3 = RetryCalculator.CalculateDelay(RetryStrategy.Linear, 3, 5);

        Assert.Equal(TimeSpan.FromSeconds(5), delay1);
        Assert.Equal(TimeSpan.FromSeconds(15), delay3);
    }

    [Fact]
    public void Exponential_GrowsProperly()
    {
        var delay1 = RetryCalculator.CalculateDelay(RetryStrategy.Exponential, 1, 5);
        var delay4 = RetryCalculator.CalculateDelay(RetryStrategy.Exponential, 4, 5);

        Assert.Equal(TimeSpan.FromSeconds(5), delay1);
        Assert.Equal(TimeSpan.FromSeconds(40), delay4);
    }

    [Fact]
    public void ExponentialWithJitter_HasVariation()
    {
        var delays = new List<TimeSpan>();
        for (int i = 0; i < 20; i++)
            delays.Add(RetryCalculator.CalculateDelay(RetryStrategy.ExponentialWithJitter, 2, 10));

        var allSame = delays.All(d => d == delays[0]);
        Assert.False(allSame);
    }
}
