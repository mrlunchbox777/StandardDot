using System;
using System.Collections.Generic;
using Moq;
using Moq.Protected;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;
using StandardDot.Enums;
using Xunit;

namespace StandardDot.Abstract.UnitTests.CoreServices
{
    public class LoggingServiceBaseTests
    {
        [Fact]
        public void ConstructorTest()
        {
            Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
            Mock<LoggingServiceBase> serviceProxy = new Mock<LoggingServiceBase>(MockBehavior.Strict, serializationServiceProxy.Object);

            Assert.NotNull(serviceProxy.Object);
        }

        [Fact]
        public void LogTest()
        {
            Log<object> log = new Log<object>();
            Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
            Mock<LoggingServiceBase> serviceProxy = new Mock<LoggingServiceBase>(MockBehavior.Loose, serializationServiceProxy.Object);
            serviceProxy.CallBase = true;
            serviceProxy.Setup(x => x.Log(log));

            serviceProxy.Object.Log(log);
            serviceProxy.Verify(x => x.Log(log), Times.AtLeastOnce());
        }

        [Fact]
        public void LogExceptionTest()
        {
            Log<object> log = null;
            Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
            Mock<LoggingServiceBase> serviceProxy = new Mock<LoggingServiceBase>(MockBehavior.Loose, serializationServiceProxy.Object);
            serviceProxy.CallBase = true;
            serviceProxy.Setup(x => x.Log(It.IsAny<Log<object>>()))
                .Callback<Log<object>>((passed) => log = passed);
            
            Exception exception = new InvalidOperationException("title");
            string message = "test";
            LogLevel level = LogLevel.Debug;
            DateTime start = DateTime.UtcNow;
            serviceProxy.Object.LogException(exception, message, level);
            DateTime end = DateTime.UtcNow;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp > start && log.TimeStamp < end);
            Assert.Equal("title", log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(level, log.LogLevel);
            Assert.Null(log.Target);
            Assert.Null(log.TargetObject);
            Assert.NotNull(log.Exception);
            Assert.Equal(exception.Message, log.Exception.Message);
            serviceProxy.Verify(x => x.Log(It.IsAny<Log<object>>()), Times.AtLeastOnce());
        }

        [Fact]
        public void LogExceptionWithObjectTest()
        {
            Log<object> log = null;
            Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
            Mock<LoggingServiceBase> serviceProxy = new Mock<LoggingServiceBase>(MockBehavior.Loose, serializationServiceProxy.Object);
            serviceProxy.CallBase = true;
            serviceProxy.Setup(x => x.Log(It.IsAny<Log<object>>()))
                .Callback<Log<object>>((passed) => log = passed);
            
            Exception exception = new InvalidOperationException("title");
            string message = "test";
            LogLevel level = LogLevel.Debug;
            object logObject = new object();
            DateTime start = DateTime.UtcNow;
            serviceProxy.Object.LogExceptionWithObject(exception, logObject, message, level);
            DateTime end = DateTime.UtcNow;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp > start && log.TimeStamp < end);
            Assert.Equal("title", log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(level, log.LogLevel);
            Assert.Equal(logObject, log.Target);
            Assert.Equal(logObject, log.TargetObject);
            Assert.NotNull(log.Exception);
            Assert.Equal(exception.Message, log.Exception.Message);
            
            serviceProxy.Verify(x => x.Log(It.IsAny<Log<object>>()), Times.AtLeastOnce());
        }

        [Fact]
        public void LogMessageTest()
        {
            Log<object> log = null;
            Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
            Mock<LoggingServiceBase> serviceProxy = new Mock<LoggingServiceBase>(MockBehavior.Loose, serializationServiceProxy.Object);
            serviceProxy.CallBase = true;
            serviceProxy.Setup(x => x.Log(It.IsAny<Log<object>>()))
                .Callback<Log<object>>((passed) => log = passed);

            string title = "testTitle";            
            string message = "test";
            LogLevel level = LogLevel.Debug;
            DateTime start = DateTime.UtcNow;
            serviceProxy.Object.LogMessage(title, message, level);
            DateTime end = DateTime.UtcNow;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp > start && log.TimeStamp < end);
            Assert.Equal(title, log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(level, log.LogLevel);
            Assert.Null(log.Target);
            Assert.Null(log.TargetObject);
            Assert.Null(log.Exception);
            
            serviceProxy.Verify(x => x.Log(It.IsAny<Log<object>>()), Times.AtLeastOnce());
        }

        [Fact]
        public void LogMessageWithObjectTest()
        {
            Log<object> log = null;
            Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
            Mock<LoggingServiceBase> serviceProxy = new Mock<LoggingServiceBase>(MockBehavior.Loose, serializationServiceProxy.Object);
            serviceProxy.CallBase = true;
            serviceProxy.Setup(x => x.Log(It.IsAny<Log<object>>()))
                .Callback<Log<object>>((passed) => log = passed);

            string title = "testTitle";            
            string message = "test";
            LogLevel level = LogLevel.Debug;
            object logObject = new object();
            DateTime start = DateTime.UtcNow;
            serviceProxy.Object.LogMessage(title, logObject, level, message);
            DateTime end = DateTime.UtcNow;

            Assert.NotNull(log);
            Assert.True(log.TimeStamp > start && log.TimeStamp < end);
            Assert.Equal(title, log.Title);
            Assert.Equal(message, log.Message);
            Assert.Equal(level, log.LogLevel);
            Assert.Equal(logObject, log.Target);
            Assert.Equal(logObject, log.TargetObject);
            Assert.Null(log.Exception);

            serviceProxy.Verify(x => x.Log(It.IsAny<Log<object>>()), Times.AtLeastOnce());
        }

        [Fact]
        public void GetLogs()
        {
            Mock<ILogBaseEnumerable> enumerableProxy = new Mock<ILogBaseEnumerable>(MockBehavior.Strict);
            Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
            Mock<LoggingServiceBase> serviceProxy = new Mock<LoggingServiceBase>(MockBehavior.Loose, serializationServiceProxy.Object);
            serviceProxy.CallBase = true;
            serviceProxy.Protected().As<LoggingServiceBaseTest>().Setup<ILogBaseEnumerable>(x => x.BaseGetLogs()).Returns(enumerableProxy.Object);

            Assert.StrictEqual(enumerableProxy.Object, serviceProxy.Object.GetLogs());
            serviceProxy.Protected().As<LoggingServiceBaseTest>().Verify<ILogBaseEnumerable>(x => x.BaseGetLogs(), Times.AtLeastOnce());
        }

        [Fact]
        public void GetLogsGeneric()
        {
            List<Log<object>> source = new List<Log<object>>(); 
            Mock<LogEnumerableBase<object>> enumerableProxy = new Mock<LogEnumerableBase<object>>(MockBehavior.Strict, source);
            Mock<ISerializationService> serializationServiceProxy = new Mock<ISerializationService>(MockBehavior.Strict);
            Mock<LoggingServiceBase> serviceProxy = new Mock<LoggingServiceBase>(MockBehavior.Loose, serializationServiceProxy.Object);
            serviceProxy.CallBase = true;
            serviceProxy.Protected().As<LoggingServiceBaseTest>().Setup<LogEnumerableBase<object>>(x => x.BaseGetLogs<object>()).Returns(enumerableProxy.Object);

            Assert.StrictEqual(enumerableProxy.Object, serviceProxy.Object.GetLogs<object>());
            serviceProxy.Protected().As<LoggingServiceBaseTest>().Verify<LogEnumerableBase<object>>(x => x.BaseGetLogs<object>(), Times.AtLeastOnce());
        }

        public abstract class LoggingServiceBaseTest
        {
            public abstract LogEnumerableBase<T> BaseGetLogs<T>()
                where T: new();

            public abstract ILogBaseEnumerable BaseGetLogs();
        }
    }
}