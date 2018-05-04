using System;
using StandardDot.Dto.CoreServices;
using StandardDot.Dto.Exception;
using StandardDot.Enums;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.Dto.UnitTests.CoreServices
{
    public class LogTests
    {
        [Fact]
        public void PropertiesTest()
        {
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
            Log<Foobar> log = new Log<Foobar>
            {
                Target = original,
                TimeStamp = DateTime.UtcNow,
                Title = title,
                Message = message,
                LogLevel = LogLevel.Info,
                Exception = new SerializableException(exception),
                Description = description
            };
            
            Assert.NotNull(log);
            Assert.True(log.TimeStamp >= log.TimeStamp.AddMilliseconds(-.5) && log.TimeStamp <= log.TimeStamp.AddMilliseconds(.5));
            Assert.Equal(title, log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(LogLevel.Info, log.LogLevel);
            Assert.Equal(description, log.Description);

            Assert.NotNull(log.Target);
            
            Assert.NotNull(log.Exception);
            Assert.Equal(typeof(SerializableException), log.Exception.GetType());
            Assert.Equal(title, log.Exception.Message);
            Assert.Equal(title, log.Exception.Message);
        }
    }
}