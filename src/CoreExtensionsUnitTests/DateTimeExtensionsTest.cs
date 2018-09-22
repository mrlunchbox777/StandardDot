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
	}
}