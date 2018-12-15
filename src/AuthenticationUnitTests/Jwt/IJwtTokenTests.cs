using System;
using Moq;
using StandardDot.Authentication.Jwt;
using Xunit;

namespace StandardDot.Authentication.UnitTests.Jwt
{
	public class IJwtTokenTests
	{
		[Fact]
		public void PropertyTests()
		{
			Mock<IJwtToken> mJwtToken = new Mock<IJwtToken>();
			mJwtToken.SetupAllProperties();
			IJwtToken token = mJwtToken.Object;
			DateTime time = DateTime.UtcNow.AddSeconds(4);
			token.Expiration = time;
			Assert.Equal(time, token.Expiration);
			mJwtToken.VerifyAll();
		}
	}
}