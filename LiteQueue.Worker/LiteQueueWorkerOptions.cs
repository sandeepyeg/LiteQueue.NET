namespace LiteQueue.Worker;

public class LiteQueueWorkerOptions
{
    public Uri? BaseAddress { get; set; }
    public string? ApiKey { get; set; }
    public string QueueName { get; set; } = string.Empty;
    public int MaxConcurrentMessages { get; set; } = 10;
    public TimeSpan VisibilityTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public TimeSpan LongPollTimeout { get; set; } = TimeSpan.FromSeconds(20);
}
