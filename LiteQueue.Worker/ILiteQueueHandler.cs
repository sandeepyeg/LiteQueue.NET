namespace LiteQueue.Worker;

public interface ILiteQueueHandler<T>
{
    Task HandleAsync(T message, CancellationToken cancellationToken);
}
