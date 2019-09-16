using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
	public class TextLogBaseEnumerableTests
	{
		[Fact]
		public void TestBasicEnumeration()
		{
			TextLoggingService service = GetLogsService();
			ClearTestLogDirectory(service);

			Tuple<Foobar, BarredFoo> objects = CreateObjects();
			service.LogMessage("Logging object 1", objects.Item1, LogLevel.Debug, "Foobar log");
			service.LogMessage("Logging object 2", objects.Item2, LogLevel.Debug, "BarredFoo log");
			ILogBaseEnumerable collection = service.GetLogs();

			Assert.NotEmpty(collection);
			foreach (LogBase log in collection)
			{
				Assert.NotNull(log);
			}
			int collectionCount = collection.Count();
			Assert.Equal(2, collectionCount);

			ClearTestLogDirectory(service);
		}

		[Fact]
		public void TestCachedEnumeration()
		{
			TextLoggingService service = GetLogsService();
			ClearTestLogDirectory(service);

			Tuple<Foobar, BarredFoo> objects = CreateObjects();
			service.LogMessage("Logging object 1", objects.Item1, LogLevel.Debug, "Foobar log");
			service.LogMessage("Logging object 2", objects.Item2, LogLevel.Debug, "BarredFoo log");
			ILogBaseEnumerable collection = service.GetLogs();

			LogBase[] logs = collection.ToArray();
			int index = 0;
			foreach (LogBase log in collection)
			{
				Assert.Equal(log, logs[index]);
				index++;
			}
			int collectionCount = collection.Count();
			Assert.Equal(2, collectionCount);

			index = 0;
			foreach (LogBase log in collection)
			{
				Assert.Equal(log, logs[index]);
				index++;
			}

			ClearTestLogDirectory(service);
		}

		[Fact]
		public void TestNonGenricEnumerator()
		{
			TextLoggingService service = GetLogsService();
			ClearTestLogDirectory(service);

			Tuple<Foobar, BarredFoo> objects = CreateObjects();
			service.LogMessage("Logging object 1", objects.Item1, LogLevel.Debug, "Foobar log");
			service.LogMessage("Logging object 2", objects.Item2, LogLevel.Debug, "BarredFoo log");
			ILogBaseEnumerable collection = service.GetLogs();

			LogBase[] logs = collection.ToArray();
			int index = 0;
			foreach (LogBase log in ((IEnumerable)collection))
			{
				Assert.Equal(log, logs[index]);
				index++;
			}
			int collectionCount = collection.Count();
			Assert.Equal(2, collectionCount);

			index = 0;
			foreach (LogBase log in ((IEnumerable)collection))
			{
				Assert.Equal(log, logs[index]);
				index++;
			}

			ClearTestLogDirectory(service);
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

		private TextLoggingService GetLogsService()
		{
			Json serializationService = new Json();
			TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);

			return loggingService;
		}

		private void ClearTestLogDirectory(TextLoggingService service)
		{
			if (!Directory.Exists(service.LogPath))
			{
				return;
			}
			List<string> allLogs = Directory.EnumerateFiles(service.LogPath).ToList();
			if (!allLogs.Any())
			{
				return;
			}
			foreach (string log in allLogs)
			{
				File.Delete(log);
			}
		}

		private string Path = Environment.CurrentDirectory + "/test/" + Guid.NewGuid().ToString("N") + "/";

		private Random random = new Random();

		private string LogExtension = ".json";
	}
}