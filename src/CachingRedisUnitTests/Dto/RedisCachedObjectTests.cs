using System;
using StandardDot.Caching.Redis.Dto;
using Xunit;

namespace StandardDot.Caching.Redis.UnitTests.Dto
{
	public class DefaultCachedObjectTests
	{
		[Fact]
		public void Properties()
		{
			RedisCachedObject<string> cachedObject = new RedisCachedObject<string>();
			cachedObject.Id.ObjectIdentifier = "test";
			const string value = "a value";
			DateTime time = DateTime.UtcNow;
			DateTime expireTime = DateTime.UtcNow.AddMinutes(5);

			cachedObject.ExpireTime = expireTime;
			cachedObject.CachedTime = time;
			cachedObject.Value = value;

			Assert.Equal(expireTime, cachedObject.ExpireTime);
			Assert.Equal(time, cachedObject.CachedTime);
			Assert.Equal(value, cachedObject.Value);
		}
	}
}