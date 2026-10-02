namespace DeskShare.UnitTests.Fakes;

internal sealed class FixedTimeProvider(DateOnly today) : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => new(today.ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero);
}
