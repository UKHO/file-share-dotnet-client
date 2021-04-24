using System;

namespace UKHO.FileShareClientTests.Helpers
{
    internal static class DateTimeExtensions
    {
        public static DateTime TruncateToMilliseconds(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
        }
    }
}