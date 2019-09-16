using System;
using Xunit;

namespace StandardDot.Constants.UnitTests
{
	public class DateTime
	{
		[Fact]
		public void BasicConstantsVerification()
		{
			Assert.Equal(new System.DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc), Constants.DateTime.UnixEpoch);
		}
	}
}
