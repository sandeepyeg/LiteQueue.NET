using Cronos;

namespace LiteQueue.Domain.Services;

public static class CronParser
{
    public static DateTimeOffset? GetNextOccurrence(string cronExpression, DateTimeOffset fromUtc, TimeZoneInfo timeZone)
    {
        try
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
            if (expression == null) return null;

            var nextUtc = expression.GetNextOccurrence(fromUtc.UtcDateTime, timeZone, true);
            if (nextUtc == null) return null;

            return new DateTimeOffset(nextUtc.Value, TimeSpan.Zero);
        }
        catch
        {
            return null;
        }
    }
}
