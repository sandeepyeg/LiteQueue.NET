using LiteQueue.Domain.Models;

namespace LiteQueue.Tests.Domain;

public class QueueDefinitionTests
{
    [Fact]
    public void NewQueueDefinition_HasDefaultValues()
    {
        var queue = new QueueDefinition();

        Assert.Equal("Active", queue.Status);
        Assert.Equal(5, queue.MaxDeliveryAttempts);
        Assert.Equal(60, queue.VisibilityTimeoutSeconds);
        Assert.Equal(259200, queue.MessageRetentionSeconds);
        Assert.Equal(1209600, queue.DeadLetterRetentionSeconds);
        Assert.Equal(262144, queue.MaxMessageSizeBytes);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        var now = DateTimeOffset.UtcNow;
        var queue = new QueueDefinition
        {
            Name = "orders",
            Status = "Paused",
            MaxDeliveryAttempts = 3,
            VisibilityTimeoutSeconds = 120,
            MessageRetentionSeconds = 86400,
            DeadLetterRetentionSeconds = 604800,
            MaxMessageSizeBytes = 524288,
            CreatedAt = now,
            UpdatedAt = now
        };

        Assert.Equal("orders", queue.Name);
        Assert.Equal("Paused", queue.Status);
        Assert.Equal(3, queue.MaxDeliveryAttempts);
        Assert.Equal(120, queue.VisibilityTimeoutSeconds);
        Assert.Equal(86400, queue.MessageRetentionSeconds);
        Assert.Equal(604800, queue.DeadLetterRetentionSeconds);
        Assert.Equal(524288, queue.MaxMessageSizeBytes);
        Assert.Equal(now, queue.CreatedAt);
        Assert.Equal(now, queue.UpdatedAt);
    }
}
