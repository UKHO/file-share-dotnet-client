using System;

namespace FileShareClientTestsCommon.Helpers
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Truncate a DateTime to millisecond level.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime TruncateToMilliseconds(this DateTime time) => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
    }
}
