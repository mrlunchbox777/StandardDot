using System;
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
    public class TextLoggingServiceTest
    {
        [Fact]
        public void LogTest()
        {
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);
            ClearTestLogDirectory(loggingService);

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogMessage("Test Log", "A test log", LogLevel.Debug);
            DateTime endLogging = DateTime.UtcNow;

            IEnumerable<string> logs = Directory.EnumerateFiles(Path);
            Assert.Single(logs);

            string fullLogName = logs.Single();
            Assert.StartsWith(Path, fullLogName);

            string logName = fullLogName.Split('\\', '/').Last();
            Assert.StartsWith(LogLevel.Debug + "_", logName);
            Assert.EndsWith(LogExtension, logName);

            string timestampString = logName.Split('_')[1];
            DateTime timeStamp = DateTime.FromFileTimeUtc(long.Parse(timestampString));
            Assert.True(timeStamp >= startLogging.AddMilliseconds(-5) && timeStamp <= endLogging.AddMilliseconds(5));

            File.Delete(logName);
            Assert.False(File.Exists(logName));
            ClearTestLogDirectory(loggingService);
        }

        [Fact]
        public void LogMessage()
        {
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);
            ClearTestLogDirectory(loggingService);

            const string title = "Test Log";
            const string message = "A test log";

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogMessage(title, message, LogLevel.Debug);
            DateTime endLogging = DateTime.UtcNow;

            IEnumerable<string> logs = Directory.EnumerateFiles(Path);
            Assert.Single(logs);

            string fullLogName = logs.Single();
            string fullLogString = File.ReadAllText(fullLogName);
            Assert.NotNull(fullLogString);

            Log<object> log = serializationService.DeserializeObject<Log<object>>(fullLogString);

            Assert.NotNull(log);
            Assert.Null(log.Target);
            Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
            Assert.Equal(title, log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(LogLevel.Debug, log.LogLevel);
            Assert.Null(log.Exception);
            Assert.Equal("Message Log", log.Description);

            ClearTestLogDirectory(loggingService);
        }

        [Fact]
        public void LogMessageWithObject()
        {
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);
            ClearTestLogDirectory(loggingService);

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

            IEnumerable<string> logs = Directory.EnumerateFiles(Path);
            Assert.Single(logs);

            string fullLogName = logs.Single();
            string fullLogString = File.ReadAllText(fullLogName);
            Assert.NotNull(fullLogString);

            Log<Foobar> log = serializationService.DeserializeObject<Log<Foobar>>(fullLogString);

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
            Assert.Equal(0, log.Target.Bar);

            ClearTestLogDirectory(loggingService);
        }

        [Fact]
        public void LogMessageWithObjectWithoutMessage()
        {
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);
            ClearTestLogDirectory(loggingService);

            const string title = "Test Log";
            Foobar original = new Foobar
                {
                    Foo = 4,
                    Bar = 6
                };

            DateTime startLogging = DateTime.UtcNow;
            loggingService.LogMessage(title, original, LogLevel.Debug);
            DateTime endLogging = DateTime.UtcNow;

            IEnumerable<string> logs = Directory.EnumerateFiles(Path);
            Assert.Single(logs);

            string fullLogName = logs.Single();
            string fullLogString = File.ReadAllText(fullLogName);
            Assert.NotNull(fullLogString);

            Log<Foobar> log = serializationService.DeserializeObject<Log<Foobar>>(fullLogString);

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
            Assert.Equal(0, log.Target.Bar);

            ClearTestLogDirectory(loggingService);
        }

        [Fact]
        public void LogException()
        {
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);
            ClearTestLogDirectory(loggingService);

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

            IEnumerable<string> logs = Directory.EnumerateFiles(Path);
            Assert.Single(logs);

            string fullLogName = logs.Single();
            string fullLogString = File.ReadAllText(fullLogName);
            Assert.NotNull(fullLogString);

            Log<object> log = serializationService.DeserializeObject<Log<object>>(fullLogString);
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

            ClearTestLogDirectory(loggingService);
        }

        [Fact]
        public void LogExceptionWithoutMessage()
        {
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);
            ClearTestLogDirectory(loggingService);

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

            IEnumerable<string> logs = Directory.EnumerateFiles(Path);
            Assert.Single(logs);

            string fullLogName = logs.Single();
            string fullLogString = File.ReadAllText(fullLogName);
            Assert.NotNull(fullLogString);

            Log<object> log = serializationService.DeserializeObject<Log<object>>(fullLogString);
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

            ClearTestLogDirectory(loggingService);
        }

        [Fact]
        public void LogExceptionWithObject()
        {
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);
            ClearTestLogDirectory(loggingService);

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

            IEnumerable<string> logs = Directory.EnumerateFiles(Path);
            Assert.Single(logs);

            string fullLogName = logs.Single();
            string fullLogString = File.ReadAllText(fullLogName);
            Assert.NotNull(fullLogString);

            Log<Foobar> log = serializationService.DeserializeObject<Log<Foobar>>(fullLogString);

            string description = "Exception Log - " + title;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
            Assert.Equal(title, log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(LogLevel.Error, log.LogLevel);
            Assert.Equal(description, log.Description);

            Assert.NotNull(log.Target);
            Assert.Equal(original.Foo, log.Target.Foo);
            Assert.Equal(0, log.Target.Bar);
            
            Assert.NotNull(log.Exception);
            Assert.Equal(typeof(SerializableException), log.Exception.GetType());
            Assert.Equal(title, log.Exception.Message);

            ClearTestLogDirectory(loggingService);
        }

        [Fact]
        public void LogExceptionWithObjectWithoutMessage()
        {
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);
            ClearTestLogDirectory(loggingService);

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

            IEnumerable<string> logs = Directory.EnumerateFiles(Path);
            Assert.Single(logs);

            string fullLogName = logs.Single();
            string fullLogString = File.ReadAllText(fullLogName);
            Assert.NotNull(fullLogString);

            Log<Foobar> log = serializationService.DeserializeObject<Log<Foobar>>(fullLogString);

            string description = "Exception Log - " + title;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
            Assert.Equal(title, log.Title);
            Assert.Equal(description, log.Message);
            Assert.Equal(LogLevel.Error, log.LogLevel);
            Assert.Equal(description, log.Description);

            Assert.NotNull(log.Target);
            Assert.Equal(original.Foo, log.Target.Foo);
            Assert.Equal(0, log.Target.Bar);
            
            Assert.NotNull(log.Exception);
            Assert.Equal(typeof(SerializableException), log.Exception.GetType());
            Assert.Equal(title, log.Exception.Message);

            ClearTestLogDirectory(loggingService);
        }

        [Fact]
        public void LogObjectTest()
        {
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);
            ClearTestLogDirectory(loggingService);

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

            IEnumerable<string> logs = Directory.EnumerateFiles(Path);
            Assert.Single(logs);

            string fullLogName = logs.Single();
            string fullLogString = File.ReadAllText(fullLogName);
            Assert.NotNull(fullLogString);

            Log<Foobar> log = serializationService.DeserializeObject<Log<Foobar>>(fullLogString);

            Assert.NotNull(log);
            Assert.True(log.TimeStamp >= log.TimeStamp.AddMilliseconds(-.5) && log.TimeStamp <= log.TimeStamp.AddMilliseconds(.5));
            Assert.Equal(originalLog.Title, log.Title);
            Assert.Equal(originalLog.Message, log.Message);
            Assert.Equal(originalLog.LogLevel, log.LogLevel);
            Assert.Equal(originalLog.Description, log.Description);

            Assert.NotNull(log.Target);
            Assert.Equal(original.Foo, log.Target.Foo);
            Assert.Equal(originalLog.Target.Foo, log.Target.Foo);
            Assert.Equal(0, log.Target.Bar);
            Assert.NotEqual(originalLog.Target.Bar, log.Target.Bar);
            Assert.Equal(original.Bar, originalLog.Target.Bar);
            
            Assert.NotNull(log.Exception);
            Assert.Equal(typeof(SerializableException), log.Exception.GetType());
            Assert.Equal(title, log.Exception.Message);
            Assert.Equal(originalLog.Exception.Message, log.Exception.Message);

            ClearTestLogDirectory(loggingService);
        }

        [Fact]
        public void GetLogsTest()
        {
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);
            ClearTestLogDirectory(loggingService);

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

            ILogEnumerable<object> logs = loggingService.GetLogs<object>();

            Assert.NotNull(logs);
            Assert.NotEmpty(logs);
            Assert.Single(logs);

            Log<object> log = logs.Single();

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

            ClearTestLogDirectory(loggingService);
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

        private string LogExtension = ".json";
    }
}