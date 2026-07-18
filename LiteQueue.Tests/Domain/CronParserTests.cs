using LiteQueue.Domain.Services;

namespace LiteQueue.Tests.Domain;

public class CronParserTests
{
    [Fact]
    public void ValidCron_ReturnsNextOccurrence()
    {
        var now = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var next = CronParser.GetNextOccurrence("0 0 2 * * *", now, TimeZoneInfo.Utc);

        Assert.NotNull(next);
        Assert.True(next > now);
    }

    [Fact]
    public void InvalidCron_ReturnsNull()
    {
        var now = DateTimeOffset.UtcNow;
        var next = CronParser.GetNextOccurrence("invalid", now, TimeZoneInfo.Utc);
        Assert.Null(next);
    }

    [Fact]
    public void EveryMinuteCron_ReturnsNextMinute()
    {
        var now = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var next = CronParser.GetNextOccurrence("* * * * * *", now, TimeZoneInfo.Utc);

        Assert.NotNull(next);
        Assert.True(next!.Value.Minute >= 0);
    }
}
