using StandardDot.CoreServices.Serialization;
using StandardDot.CoreServices.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.TestClasses.AbstractImplementations;

namespace StandardDot.CoreServices.UnitTests.Logging
{
	public class TestMemoryCacheProvider
	{
		public static CacheLoggingService GetLogsService(ISerializationService serializationService = null, TimeSpan? cacheLife = null)
		{
			serializationService = serializationService ?? new Json();
			ICachingService cachingService = new TestMemoryCachingService(cacheLife ?? TimeSpan.FromMinutes(5));
			CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService);

			return loggingService;
		}
	}
}