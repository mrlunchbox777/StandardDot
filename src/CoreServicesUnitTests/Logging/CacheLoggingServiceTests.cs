using System;
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

namespace StandardDot.CoreServices.UnitTests.Logging
{
    public class CacheLoggingServiceTest
    {
        [Fact]
        public void LogTest()
        {
            TestMemoryCache cachingService = new TestMemoryCache(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService, _partSeperator);

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogMessage("Test Log", "A test log", LogLevel.Debug);
            DateTime endLogging = DateTime.UtcNow;

            Assert.Single(cachingService);

            string fullLogName = cachingService.Single().Key;
            Assert.StartsWith(LogLevel.Debug.ToString() + _partSeperator, fullLogName);

            string timestampString = fullLogName.Split('_')[1];
            DateTime timeStamp = DateTime.FromFileTimeUtc(long.Parse(timestampString));
            Assert.True(timeStamp >= startLogging.AddMilliseconds(-5) && timeStamp <= endLogging.AddMilliseconds(5));
        }

        [Fact]
        public void LogMessage()
        {
            TestMemoryCache cachingService = new TestMemoryCache(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService, _partSeperator);

            const string title = "Test Log";
            const string message = "A test log";

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogMessage(title, message, LogLevel.Debug);
            DateTime endLogging = DateTime.UtcNow;

            Assert.Single(cachingService);

            string fullLogName = cachingService.Single().Key;
            Assert.NotNull(fullLogName);

            Log<object> log = (Log<object>)cachingService.Single().Value.Value;

            Assert.NotNull(log);
            Assert.Null(log.Target);
            Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
            Assert.Equal(title, log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(LogLevel.Debug, log.LogLevel);
            Assert.Null(log.Exception);
            Assert.Equal("Message Log", log.Description);
        }

        [Fact]
        public void LogMessageWithObject()
        {
            TestMemoryCache cachingService = new TestMemoryCache(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService, _partSeperator);

            const string title = "Test Log";
            const string message = "A test log";
            Foobar original = new Foobar
                {
                    Foo = 4,
                    Bar = 6
                };

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogMessage(title, original, LogLevel.Debug, message);
            DateTime endLogging = DateTime.UtcNow;

            Assert.Single(cachingService);

            string fullLogName = cachingService.Single().Key;
            Assert.NotNull(fullLogName);

            Log<Foobar> log = (Log<Foobar>)cachingService.Single().Value.Value;

            string description = "Message Log with object - " + typeof(Foobar).FullName;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
            Assert.Equal(title, log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(LogLevel.Debug, log.LogLevel);
            Assert.Null(log.Exception);
            Assert.Equal(description, log.Description);

            Assert.NotNull(log.Target);
            Assert.Equal(original.Foo, log.Target.Foo);
            // The memory caching service never serializes or deserializes so the non-serialized values won't change
            //Assert.Equal(0, log.Target.Bar);
        }

        [Fact]
        public void LogMessageWithObjectWithoutMessage()
        {
            TestMemoryCache cachingService = new TestMemoryCache(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService, _partSeperator);

            const string title = "Test Log";
            Foobar original = new Foobar
                {
                    Foo = 4,
                    Bar = 6
                };

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogMessage(title, original, LogLevel.Debug);
            DateTime endLogging = DateTime.UtcNow;

            Assert.Single(cachingService);

            string fullLogName = cachingService.Single().Key;
            Assert.NotNull(fullLogName);

            Log<Foobar> log = (Log<Foobar>)cachingService.Single().Value.Value;

            string description = "Message Log with object - " + typeof(Foobar).FullName;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
            Assert.Equal(title, log.Title);
            Assert.Equal(description, log.Message);
            Assert.Equal(LogLevel.Debug, log.LogLevel);
            Assert.Null(log.Exception);
            Assert.Equal(description, log.Description);

            Assert.NotNull(log.Target);
            Assert.Equal(original.Foo, log.Target.Foo);
            // The memory caching service never serializes or deserializes so the non-serialized values won't change
            // Assert.Equal(0, log.Target.Bar);
        }

        [Fact]
        public void LogException()
        {
            TestMemoryCache cachingService = new TestMemoryCache(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService, _partSeperator);

            const string title = "Test Log";
            const string message = "A test log";

            InvalidOperationException exception;
            try
            {
                throw new InvalidOperationException(title);
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogException(exception, message);
            DateTime endLogging = DateTime.UtcNow;

            Assert.Single(cachingService);

            string fullLogName = cachingService.Single().Key;
            Assert.NotNull(fullLogName);

            Log<object> log = (Log<object>)cachingService.Single().Value.Value;
            string description = "Exception Log - " + title;

            Assert.NotNull(log);
            Assert.Null(log.Target);
            Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
            Assert.Equal(title, log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(LogLevel.Error, log.LogLevel);
            Assert.Equal(description, log.Description);

            Assert.NotNull(log.Exception);
            Assert.Equal(typeof(SerializableException), log.Exception.GetType());
            Assert.Equal(title, log.Exception.Message);
        }

        [Fact]
        public void LogExceptionWithoutMessage()
        {
            TestMemoryCache cachingService = new TestMemoryCache(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService, _partSeperator);

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

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogException(exception);
            DateTime endLogging = DateTime.UtcNow;

            Assert.Single(cachingService);

            string fullLogName = cachingService.Single().Key;
            Assert.NotNull(fullLogName);

            Log<object> log = (Log<Object>)cachingService.Single().Value.Value;
            string description = "Exception Log - " + title;

            Assert.NotNull(log);
            Assert.Null(log.Target);
            Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
            Assert.Equal(title, log.Title);
            Assert.Equal(description, log.Message);
            Assert.Equal(LogLevel.Error, log.LogLevel);
            Assert.Equal(description, log.Description);

            Assert.NotNull(log.Exception);
            Assert.Equal(typeof(SerializableException), log.Exception.GetType());
            Assert.Equal(title, log.Exception.Message);
        }

        [Fact]
        public void LogExceptionWithObject()
        {
            TestMemoryCache cachingService = new TestMemoryCache(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService, _partSeperator);

            const string title = "Test Log";
            const string message = "A test log";
            Foobar original = new Foobar
                {
                    Foo = 4,
                    Bar = 6
                };

            InvalidOperationException exception;
            try
            {
                throw new InvalidOperationException(title);
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogExceptionWithObject(exception, original, message);
            DateTime endLogging = DateTime.UtcNow;

            Assert.Single(cachingService);

            string fullLogName = cachingService.Single().Key;
            Assert.NotNull(fullLogName);

            Log<Foobar> log = (Log<Foobar>)cachingService.Single().Value.Value;

            string description = "Exception Log - " + title;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
            Assert.Equal(title, log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(LogLevel.Error, log.LogLevel);
            Assert.Equal(description, log.Description);

            Assert.NotNull(log.Target);
            Assert.Equal(original.Foo, log.Target.Foo);
            // The memory caching service never serializes or deserializes so the non-serialized values won't change
            // Assert.Equal(0, log.Target.Bar);
            
            Assert.NotNull(log.Exception);
            Assert.Equal(typeof(SerializableException), log.Exception.GetType());
            Assert.Equal(title, log.Exception.Message);
        }

        [Fact]
        public void LogExceptionWithObjectWithoutMessage()
        {
            TestMemoryCache cachingService = new TestMemoryCache(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService, _partSeperator);

            const string title = "Test Log";
            Foobar original = new Foobar
                {
                    Foo = 4,
                    Bar = 6
                };

            InvalidOperationException exception;
            try
            {
                throw new InvalidOperationException(title);
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogExceptionWithObject(exception, original);
            DateTime endLogging = DateTime.UtcNow;

            Assert.Single(cachingService);

            string fullLogName = cachingService.Single().Key;
            Assert.NotNull(fullLogName);

            Log<Foobar> log = (Log<Foobar>)cachingService.Single().Value.Value;

            string description = "Exception Log - " + title;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
            Assert.Equal(title, log.Title);
            Assert.Equal(description, log.Message);
            Assert.Equal(LogLevel.Error, log.LogLevel);
            Assert.Equal(description, log.Description);

            Assert.NotNull(log.Target);
            Assert.Equal(original.Foo, log.Target.Foo);
            // The memory caching service never serializes or deserializes so the non-serialized values won't change
            // Assert.Equal(0, log.Target.Bar);
            
            Assert.NotNull(log.Exception);
            Assert.Equal(typeof(SerializableException), log.Exception.GetType());
            Assert.Equal(title, log.Exception.Message);
        }

        [Fact]
        public void LogObjectTest()
        {
            TestMemoryCache cachingService = new TestMemoryCache(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService, _partSeperator);

            const string title = "Test Log";
            const string message = "A test log";
            Foobar original = new Foobar
                {
                    Foo = 4,
                    Bar = 6
                };

            InvalidOperationException exception;
            try
            {
                throw new InvalidOperationException(title);
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }
            
            string description = "Manual Exception Log - " + message;
            Log<Foobar> originalLog = new Log<Foobar>
            {
                Target = original,
                TimeStamp = DateTime.UtcNow,
                Title = title,
                Message = message,
                LogLevel = LogLevel.Info,
                Exception = new SerializableException(exception),
                Description = description
            };

            loggingService.Log(originalLog);

            Assert.Single(cachingService);

            string fullLogName = cachingService.Single().Key;
            Assert.NotNull(fullLogName);

            Log<Foobar> log = (Log<Foobar>)cachingService.Single().Value.Value;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp >= log.TimeStamp.AddMilliseconds(-.5) && log.TimeStamp <= log.TimeStamp.AddMilliseconds(.5));
            Assert.Equal(originalLog.Title, log.Title);
            Assert.Equal(originalLog.Message, log.Message);
            Assert.Equal(originalLog.LogLevel, log.LogLevel);
            Assert.Equal(originalLog.Description, log.Description);

            Assert.NotNull(log.Target);
            Assert.Equal(original.Foo, log.Target.Foo);
            Assert.Equal(originalLog.Target.Foo, log.Target.Foo);
            // The memory caching service never serializes or deserializes so the non-serialized values won't change
            // Assert.Equal(0, log.Target.Bar);
            // The memory caching service never serializes or deserializes so the non-serialized values won't change
            // Assert.NotEqual(originalLog.Target.Bar, log.Target.Bar);
            Assert.Equal(originalLog.Target.Bar, log.Target.Bar);
            Assert.Equal(original.Bar, originalLog.Target.Bar);
            
            Assert.NotNull(log.Exception);
            Assert.Equal(typeof(SerializableException), log.Exception.GetType());
            Assert.Equal(title, log.Exception.Message);
            Assert.Equal(originalLog.Exception.Message, log.Exception.Message);
        }

        [Fact]
        public void GetLogsTest()
        {
            TestMemoryCache cachingService = new TestMemoryCache(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(cachingService, serializationService, _partSeperator);

            const string title = "Test Log";
            const string message = "A test log";
            Foobar original = new Foobar
                {
                    Foo = 4,
                    Bar = 6
                };

            InvalidOperationException exception;
            try
            {
                throw new InvalidOperationException(title);
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }
            
            string description = "Manual Exception Log - " + message;
            Log<Foobar> originalLog = new Log<Foobar>
            {
                Target = original,
                TimeStamp = DateTime.UtcNow,
                Title = title,
                Message = message,
                LogLevel = LogLevel.Info,
                Exception = new SerializableException(exception),
                Description = description
            };

            loggingService.Log(originalLog);

            ILogEnumerable<Foobar> logs = loggingService.GetLogs<Foobar>();

            Assert.NotNull(logs);
            Assert.NotEmpty(logs);
            Assert.Single(logs);

            Log<Foobar> log = logs.Single();

            Assert.NotNull(log);
            Assert.True(log.TimeStamp >= log.TimeStamp.AddMilliseconds(-.5) && log.TimeStamp <= log.TimeStamp.AddMilliseconds(.5));
            Assert.Equal(originalLog.Title, log.Title);
            Assert.Equal(originalLog.Message, log.Message);
            Assert.Equal(originalLog.LogLevel, log.LogLevel);
            Assert.Equal(originalLog.Description, log.Description);

            Assert.NotNull(log.Target);
            
            Assert.NotNull(log.Exception);
            Assert.Equal(typeof(SerializableException), log.Exception.GetType());
            Assert.Equal(title, log.Exception.Message);
            Assert.Equal(originalLog.Exception.Message, log.Exception.Message);

            Assert.Equal(log, logs.First());
        }

        private string _partSeperator = "_";
    }
}