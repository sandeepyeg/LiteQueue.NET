namespace LiteQueue.Domain.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
