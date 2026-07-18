using LiteQueue.Domain.Models;

namespace LiteQueue.Tests.Domain;

public class DeadLetterMessageTests
{
    [Fact]
    public void NewDeadLetterMessage_HasUtcNowDeadLetteredAt()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var message = new DeadLetterMessage();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        Assert.True(message.DeadLetteredAt >= before);
        Assert.True(message.DeadLetteredAt <= after);
    }

    [Fact]
    public void CanTrackFailureDetails()
    {
        var originalId = Guid.NewGuid().ToString();
        var message = new DeadLetterMessage
        {
            Id = "dlq-123",
            OriginalQueueName = "emails",
            OriginalMessageId = originalId,
            Body = "{\"to\":\"test@test.com\"}",
            DeliveryAttempts = 5,
            LastError = "Connection timeout",
            DeadLetteredAt = DateTimeOffset.UtcNow
        };

        Assert.Equal("dlq-123", message.Id);
        Assert.Equal("emails", message.OriginalQueueName);
        Assert.Equal(originalId, message.OriginalMessageId);
        Assert.Equal(5, message.DeliveryAttempts);
        Assert.Equal("Connection timeout", message.LastError);
    }
}
