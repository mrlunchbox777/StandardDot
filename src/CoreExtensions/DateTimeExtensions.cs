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

		/// <summary>
		/// Compares two DateTimes with a tolerance, default 1 second
		/// </summary>
		/// <param name="source">The first DateTime</param>
		/// <param name="other">The second DateTime</param>
		/// <param name="tolerance">The tolerance that is acceptable for equality</param>
		/// <returns>If the DateTimes are equal within a tolerance</returns>
		public static bool Compare(this DateTime source, DateTime other, TimeSpan? tolerance = null)
		{
			if (source == other)
			{
				return true;
			}

			if (source == null || other == null)
			{
				return false;
			}

			return (source - other) <= (tolerance ?? new TimeSpan(0, 0, 1));
		}
	}
}