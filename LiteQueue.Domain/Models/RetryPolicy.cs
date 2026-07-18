using LiteQueue.Domain.Enums;

namespace LiteQueue.Domain.Models;

public class RetryPolicy
{
    public RetryStrategy Strategy { get; set; } = RetryStrategy.Immediate;
    public int MaxDeliveryAttempts { get; set; } = 5;
    public int InitialDelaySeconds { get; set; } = 5;
    public int MaximumDelaySeconds { get; set; } = 300;
}
