namespace LiteQueue.Client;

public class LiteQueueClientOptions
{
    public Uri? BaseAddress { get; set; }
    public string? ApiKey { get; set; }
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
