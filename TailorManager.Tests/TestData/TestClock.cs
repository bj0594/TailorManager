namespace TailorManager.Tests;

// A clock the tests set, so the time in the history (and later fittings and payments) is known.
//   _app.Clock.Advance(TimeSpan.FromHours(1));
public class TestClock : TimeProvider
{
    // 2 November 2026 at 09:00 UTC, the first day of the project.
    public DateTimeOffset Now { get; set; } = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => Now;

    public void Advance(TimeSpan time) => Now += time;
}
