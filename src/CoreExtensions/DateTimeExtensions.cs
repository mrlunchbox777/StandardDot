using System;

namespace StandardDot.CoreExtensions
{
    /// <summary>
    /// Extensions for DateTimes
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets the Unix Time Stamp for a DateTime
        /// </summary>
        /// <param name="source">The DateTime</param>
        /// <returns>The Unix Time Stamp for a Date Time</returns>
        public static ulong UnixTimeStamp(this DateTime source)
        {
            return (ulong)(source - Constants.DateTime.UnixEpoch).TotalSeconds;
        }

        /// <summary>
        /// Gets a DateTime from a Unix Timestamp
        /// </summary>
        /// <param name="source">The Unix Timestamp</param>
        /// <returns>The Date Time for a Unix Time Stamp</returns>
        public static DateTime? FromUnixTimeStamp(this ulong source)
        {
            return Constants.DateTime.UnixEpoch.Add(TimeSpan.FromSeconds(source));
        }
    }
}