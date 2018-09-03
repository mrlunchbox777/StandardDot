using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StandardDot.Abstract.CoreServices;
using StandardDot.Abstract.CoreServices.Serialization;
using StandardDot.CoreServices.Logging;
using StandardDot.Dto.CoreServices;
using StandardDot.Dto.Exception;
using StandardDot.Enums;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.CoreServices.IntegrationTests.Logging
{
    public class TextLogEnumerableTests
    {
        [Fact]
        public void TestBasicEnumeration()
        {
            TextLoggingService service = GetLogsService();

            ClearTestLogDirectory(service);
            Tuple<Foobar, BarredFoo> objects = CreateObjects();
            service.LogMessage("Logging object 1", objects.Item1, LogLevel.Debug, "Foobar log");
            service.LogMessage("Logging object 2", objects.Item2, LogLevel.Debug, "BarredFoo log");
            ILogEnumerable<object> collection = service.GetLogs<object>();

            Assert.NotEmpty(collection);
            foreach (Log<object> log in collection)
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
            ILogEnumerable<object> collection = service.GetLogs<object>();

            Log<object>[] logs = collection.ToArray();
            int index = 0;
            foreach (Log<object> log in collection)
            {
                Assert.Equal(log, logs[index]);
                index++;
            }
            int collectionCount = collection.Count(); 
            Assert.Equal(2, collectionCount);
            
            index = 0; 
            foreach (Log<object> log in collection)
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
            ILogEnumerable<object> collection = service.GetLogs<object>();

            Log<object>[] logs = collection.ToArray();
            int index = 0;
            foreach (Log<object> log in ((IEnumerable)collection))
            {
                Assert.Equal(log, logs[index]);
                index++;
            }
            int collectionCount = collection.Count(); 
            Assert.Equal(2, collectionCount);
            
            index = 0; 
            foreach (Log<object> log in ((IEnumerable)collection))
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

        private Log<object> AddMessageLog(TextLoggingService loggingService)
        {
            return AddMessageLogWithObject<object>(null, loggingService);
        }

        private Log<T> AddMessageLogWithObject<T>(T target, TextLoggingService loggingService)
            where T : new()
        {
            return AddLog(target, null, loggingService);
        }

        public Log<object> AddExceptionLog(TextLoggingService loggingService)
        {
            return AddExceptionLogWithObject<object>(null, loggingService);
        }

        public Log<T> AddExceptionLogWithObject<T>(T target, TextLoggingService loggingService)
            where T : new()
        {
            const string title = "Test Log";
            InvalidOperationException exception;
            try
            {
                throw new InvalidOperationException(title);
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }
            return AddLog(target, exception, loggingService);
        }

        private Log<T> AddLog<T>(T target, Exception exception, TextLoggingService loggingService)
            where T : new()
        {
            const string title = "Test Log";
            const string message = "A test log";

            string description = "Manual Exception Log - " + message;
            Log<T> originalLog = new Log<T>
            {
                Target = target,
                TimeStamp = DateTime.UtcNow,
                Title = title,
                Message = message,
                LogLevel = LogLevel.Info,
                Exception = exception == null ? null : new SerializableException(exception),
                Description = description
            };

            loggingService.Log(originalLog);

            return originalLog;
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