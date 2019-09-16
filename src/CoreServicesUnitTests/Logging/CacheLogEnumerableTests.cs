using System;
using System.Collections;
using System.Linq;
using StandardDot.Abstract.CoreServices;
using StandardDot.CoreServices.Serialization;
using StandardDot.CoreServices.Logging;
using StandardDot.Dto.CoreServices;
using StandardDot.Dto.Exception;
using StandardDot.Enums;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.CoreServices.UnitTests.Logging
{
	public class CacheLogEnumerableTests
	{
		[Fact]
		public void TestBasicEnumeration()
		{
			CacheLoggingService service = TestMemoryCacheProvider.GetLogsService();
			Tuple<Foobar, BarredFoo> objects = CreateObjects();
			service.LogMessage("Logging object 1", objects.Item1, LogLevel.Debug, "Foobar log");
			service.LogMessage("Logging object 2", objects.Item2, LogLevel.Debug, "BarredFoo log");
			ILogEnumerable<Foobar> collection = service.GetLogs<Foobar>();

			Assert.NotEmpty(collection);
			foreach (Log<Foobar> log in collection)
			{
				Assert.NotNull(log);
			}
			int collectionCount = collection.Count();
			Assert.Equal(1, collectionCount);

			ILogEnumerable<BarredFoo> collection2 = service.GetLogs<BarredFoo>();

			Assert.NotEmpty(collection);
			foreach (Log<BarredFoo> log in collection2)
			{
				Assert.NotNull(log);
			}
			int collectionCount2 = collection2.Count();
			Assert.Equal(1, collectionCount2);
		}

		[Fact]
		public void TestNonGenricEnumerator()
		{
			CacheLoggingService service = TestMemoryCacheProvider.GetLogsService();
			Tuple<Foobar, BarredFoo> objects = CreateObjects();
			service.LogMessage("Logging object 1", objects.Item1, LogLevel.Debug, "Foobar log");
			service.LogMessage("Logging object 2", objects.Item2, LogLevel.Debug, "BarredFoo log");
			ILogEnumerable<Foobar> collection = service.GetLogs<Foobar>();

			Log<Foobar>[] logs = collection.ToArray();
			int index = 0;
			foreach (Log<Foobar> log in ((IEnumerable)collection))
			{
				Assert.Equal(log, logs[index]);
				index++;
			}
			int collectionCount = collection.Count();
			Assert.Equal(1, collectionCount);

			index = 0;
			foreach (Log<Foobar> log in ((IEnumerable)collection))
			{
				Assert.Equal(log, logs[index]);
				index++;
			}
		}

		public Tuple<Foobar, BarredFoo> CreateObjects()
		{
			Foobar original = new Foobar
			{
				Foo = random.Next(-10000, 10000),
				Bar = random.Next(-10000, 10000)
			};

			BarredFoo original2 = new BarredFoo
			{
				Foo = random.Next(-10000, 10000),
				Barred = random.Next(-10000, 10000)
			};

			return new Tuple<Foobar, BarredFoo>(original, original2);
		}

		private Random random = new Random();
	}
}