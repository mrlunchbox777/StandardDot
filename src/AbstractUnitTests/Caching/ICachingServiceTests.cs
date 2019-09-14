using System;
using Moq;
using StandardDot.Abstract.Caching;
using Xunit;

namespace StandardDot.Abstract.UnitTests.Caching
{
	public class ICachingServiceTests
	{
		[Fact]
		public void PropertiesTest()
		{
			Mock<ICachingService> cachingService = GetService();
			Assert.Equal(300, (int)cachingService.Object.DefaultCacheLifespan.TotalSeconds);
		}

		[Fact]
		public void CacheTest()
		{
			Mock<ICachingService> cachingService = GetService();
			ICachedObject<string> cachedObject = GetCachedObject();

			cachingService.Setup(x => x.Cache(key, cachedObject));

			cachingService.Object.Cache(key, cachedObject);
		}

		[Fact]
		public void CacheTest2()
		{
			Mock<ICachingService> cachingService = GetService();
			ICachedObject<string> cachedObject = GetCachedObject();

			cachingService.Setup(x => x.Cache(key, cachedObject.Value, cachedObject.CachedTime, cachedObject.ExpireTime));

			cachingService.Object.Cache(key, cachedObject.Value, cachedObject.CachedTime, cachedObject.ExpireTime);
		}

		[Fact]
		public void RetrieveTest()
		{
			Mock<ICachingService> cachingService = GetService();
			ICachedObject<string> cachedObject = GetCachedObject();

			cachingService.Setup(x => x.Retrieve<string>(key)).Returns(cachedObject);

			Assert.Equal(cachedObject, cachingService.Object.Retrieve<string>(key));
		}

		[Fact]
		public void InvalidateTest()
		{
			Mock<ICachingService> cachingService = GetService();
			ICachedObject<string> cachedObject = GetCachedObject();

			cachingService.Setup(x => x.Invalidate(key)).Returns(true);

			Assert.True(cachingService.Object.Invalidate(key));
		}

		private const string key = "a key";

		private const string value = "a value";

		private DateTime time = DateTime.UtcNow;

		private DateTime expireTime = DateTime.UtcNow.AddMinutes(5);

		private ICachedObject<string> GetCachedObject()
		{
			Mock<ICachedObject<string>> cachedObjectProxy = new Mock<ICachedObject<string>>();
			cachedObjectProxy.SetupAllProperties();
			ICachedObject<string> cachedObject = cachedObjectProxy.Object;

			cachedObject.ExpireTime = expireTime;
			cachedObject.CachedTime = time;
			cachedObject.Value = value;

			return cachedObject;
		}

		private Mock<ICachingService> GetService()
		{
			Mock<ICachingService> cachingServiceProxy = new Mock<ICachingService>(MockBehavior.Strict);
			TimeSpan defaultCacheTime = TimeSpan.FromMinutes(5);
			cachingServiceProxy.SetupGet(x => x.DefaultCacheLifespan).Returns(defaultCacheTime);
			return cachingServiceProxy;
		}
	}
}