using System;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests
{
	public class DateTimeExtensionsTest
	{
		[Fact]
		public void UnixTimeStampTest()
		{
			DateTime epoch = Constants.DateTime.UnixEpoch;
			DateTime epochPlusAMinute = epoch.AddMinutes(1);
			Assert.Equal((ulong)0, epoch.UnixTimeStamp());
			Assert.Equal((ulong)60, epochPlusAMinute.UnixTimeStamp());
		}

		[Fact]
		public void FromUnixTimeStampTest()
		{
			DateTime epoch = Constants.DateTime.UnixEpoch;
			DateTime epochPlusAMinute = epoch.AddMinutes(1);
			Assert.Equal(epoch, ((ulong)0).FromUnixTimeStamp());
			Assert.Equal(epochPlusAMinute, ((ulong)60).FromUnixTimeStamp());
		}

		[Fact]
		public void CompareDateTimeTest()
		{
			DateTime first = DateTime.UtcNow;
			DateTime second = first.AddSeconds(5);
			TimeSpan passTolerance = TimeSpan.FromSeconds(5);

			Assert.False(first.Compare(second));
			Assert.True(first.Compare(second, passTolerance));

			Assert.False(second.Compare(first));
			Assert.True(second.Compare(first, passTolerance));
		}
	}
}