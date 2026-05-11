namespace Real_Estate_App.Extensions
{
    public static class DateTimeExtensions
    {
        // Transaction dates are stored as UTC (DateTime.UtcNow at write sites).
        // EF reads them back with Kind=Unspecified, so ToLocalTime() alone is a
        // no-op. SpecifyKind first, then convert for display.
        public static DateTime ToLocalDisplay(this DateTime utc) =>
            DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();
    }
}
