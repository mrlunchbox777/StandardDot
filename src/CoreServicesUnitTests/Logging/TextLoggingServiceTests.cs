using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StandardDot.Abstract.CoreServices.Serialization;
using StandardDot.CoreServices.Logging;
using StandardDot.Dto.CoreServices;
using StandardDot.Dto.Exception;
using StandardDot.Enums;
using Xunit;

namespace StandardDot.CoreServices.UnitTests.Logging
{
    public class TextLoggingServiceTest
    {
        [Fact]
        public void LogTest()
        {
            ClearTestLogDirectory();
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);

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
            ClearTestLogDirectory();
        }

        [Fact]
        public void LogMessage()
        {
            ClearTestLogDirectory();
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);

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

            ClearTestLogDirectory();
        }

        [Fact]
        public void LogMessageWithObject()
        {
            ClearTestLogDirectory();
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);

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

            ClearTestLogDirectory();
        }

        [Fact]
        public void LogMessageWithObjectWithoutMessage()
        {
            ClearTestLogDirectory();
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);

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

            ClearTestLogDirectory();
        }

        [Fact]
        public void LogException()
        {
            ClearTestLogDirectory();
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);

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

            ClearTestLogDirectory();
        }

        [Fact]
        public void LogExceptionWithoutMessage()
        {
            ClearTestLogDirectory();
            Json serializationService = new Json();
            TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);

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

            ClearTestLogDirectory();
        }

        // [Fact]
        // public void LogMessageWithObject()
        // {
        //     ClearTestLogDirectory();
        //     Json serializationService = new Json();
        //     TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);

        //     const string title = "Test Log";
        //     const string message = "A test log";
        //     Foobar original = new Foobar
        //         {
        //             Foo = 4,
        //             Bar = 6
        //         };

        //     DateTime startLogging = DateTime.UtcNow;
        //     loggingService.LogMessage(title, original, LogLevel.Debug, message);
        //     DateTime endLogging = DateTime.UtcNow;

        //     IEnumerable<string> logs = Directory.EnumerateFiles(Path);
        //     Assert.Single(logs);

        //     string fullLogName = logs.Single();
        //     string fullLogString = File.ReadAllText(fullLogName);
        //     Assert.NotNull(fullLogString);

        //     Log<Foobar> log = serializationService.DeserializeObject<Log<Foobar>>(fullLogString);

        //     string description = "Message Log with object - " + typeof(Foobar).FullName;

        //     Assert.NotNull(log);
        //     Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
        //     Assert.Equal(title, log.Title);
        //     Assert.Equal(message, log.Message);
        //     Assert.Equal(LogLevel.Debug, log.LogLevel);
        //     Assert.Null(log.Exception);
        //     Assert.Equal(description, log.Description);

        //     Assert.NotNull(log.Target);
        //     Assert.Equal(original.Foo, log.Target.Foo);
        //     Assert.Equal(0, log.Target.Bar);

        //     ClearTestLogDirectory();
        // }

        // [Fact]
        // public void LogMessageWithObjectWithoutMessage()
        // {
        //     ClearTestLogDirectory();
        //     Json serializationService = new Json();
        //     TextLoggingService loggingService = new TextLoggingService(Path, serializationService, LogExtension);

        //     const string title = "Test Log";
        //     Foobar original = new Foobar
        //         {
        //             Foo = 4,
        //             Bar = 6
        //         };

        //     DateTime startLogging = DateTime.UtcNow;
        //     loggingService.LogMessage(title, original, LogLevel.Debug);
        //     DateTime endLogging = DateTime.UtcNow;

        //     IEnumerable<string> logs = Directory.EnumerateFiles(Path);
        //     Assert.Single(logs);

        //     string fullLogName = logs.Single();
        //     string fullLogString = File.ReadAllText(fullLogName);
        //     Assert.NotNull(fullLogString);

        //     Log<Foobar> log = serializationService.DeserializeObject<Log<Foobar>>(fullLogString);

        //     string description = "Message Log with object - " + typeof(Foobar).FullName;

        //     Assert.NotNull(log);
        //     Assert.True(log.TimeStamp >= startLogging.AddMilliseconds(-5) && log.TimeStamp <= endLogging.AddMilliseconds(5));
        //     Assert.Equal(title, log.Title);
        //     Assert.Equal(description, log.Message);
        //     Assert.Equal(LogLevel.Debug, log.LogLevel);
        //     Assert.Null(log.Exception);
        //     Assert.Equal(description, log.Description);

        //     Assert.NotNull(log.Target);
        //     Assert.Equal(original.Foo, log.Target.Foo);
        //     Assert.Equal(0, log.Target.Bar);

        //     ClearTestLogDirectory();
        // }

        [Fact]
        public void LogObject()
        {
            Json service = new Json();
            string originalString = "{\"Foo\":4, \"Bar\":3}";
            Foobar original = new Foobar
                {
                    Foo = 4
                };
            
            Foobar deserialized = service.DeserializeObject<Foobar>(originalString);
            Assert.NotNull(deserialized);
            Assert.NotEqual(original, deserialized);
            Assert.Equal(original.Foo, deserialized.Foo);
            Assert.Equal(original.Bar, deserialized.Bar);
        }

        private void ClearTestLogDirectory()
        {
            if (!Directory.Exists(Path))
            {
                return;
            }
            List<string> allLogs = Directory.EnumerateFiles(Path).ToList();
            if (!allLogs.Any())
            {
                return;
            }
            foreach (string log in allLogs)
            {
                File.Delete(log);
            }
        }

        private string Path = Environment.CurrentDirectory + "/test/";

        private string LogExtension = ".json";
    }
}