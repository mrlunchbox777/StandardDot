using System;
using System.Collections;
using System.Collections.Generic;
using Moq;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;
using StandardDot.Enums;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.Abstract.IntegrationTests.CoreServices
{
	public class ILoggingServiceTests
	{
		[Fact]
		public void LogTest()
		{
			Log<object> log = new Log<object>();
			Mock<ILoggingService> loggingService = new Mock<ILoggingService>(MockBehavior.Strict);
			loggingService.Setup(x => x.Log(log));
			loggingService.Object.Log(log);
		}

		[Fact]
		public void LogExceptionTest()
		{
			InvalidOperationException exception = new InvalidOperationException("test");
			LogLevel level = LogLevel.Debug;
			string message = "this is a test";
			Mock<ILoggingService> loggingService = new Mock<ILoggingService>(MockBehavior.Strict);
			loggingService.Setup(x => x.LogException(exception, message, level));
			loggingService.Object.LogException(exception, message, level);
		}

		[Fact]
		public void LogExceptionWithObjectTest()
		{
			InvalidOperationException exception = new InvalidOperationException("test");
			LogLevel level = LogLevel.Debug;
			string message = "this is a test";
			Foobar foobar = new Foobar { Foo = 4, Bar = 3 };
			Mock<ILoggingService> loggingService = new Mock<ILoggingService>(MockBehavior.Strict);
			loggingService.Setup(x => x.LogExceptionWithObject(exception, foobar, message, level));
			loggingService.Object.LogExceptionWithObject(exception, foobar, message, level);
		}

		[Fact]
		public void LogMessageTest()
		{
			string title = "test";
			LogLevel level = LogLevel.Debug;
			string message = "this is a test";
			Mock<ILoggingService> loggingService = new Mock<ILoggingService>(MockBehavior.Strict);
			loggingService.Setup(x => x.LogMessage(title, message, level));
			loggingService.Object.LogMessage(title, message, level);
		}

		[Fact]
		public void LogMessageWithObjectTest()
		{
			string title = "test";
			LogLevel level = LogLevel.Debug;
			string message = "this is a test";
			Foobar foobar = new Foobar { Foo = 4, Bar = 3 };
			Mock<ILoggingService> loggingService = new Mock<ILoggingService>(MockBehavior.Strict);
			loggingService.Setup(x => x.LogMessage(title, foobar, level, message));
			loggingService.Object.LogMessage(title, foobar, level, message);
		}

		[Fact]
		public void GetLogsGenericTest()
		{
			Mock<ILogEnumerable<object>> logsProxy = new Mock<ILogEnumerable<object>>(MockBehavior.Strict);
			ILogEnumerable<object> logs = logsProxy.Object;
			Mock<ILoggingService> loggingService = new Mock<ILoggingService>(MockBehavior.Strict);
			loggingService.Setup(x => x.GetLogs<object>()).Returns(logs);
			ILogEnumerable<object> result = loggingService.Object.GetLogs<object>();
			Assert.NotNull(result);
			// use strict equal to do a pointer comparison
			Assert.StrictEqual(logs, result);
		}

		[Fact]
		public void GetLogsTest()
		{
			Mock<ILogBaseEnumerable> logsProxy = new Mock<ILogBaseEnumerable>(MockBehavior.Strict);
			ILogBaseEnumerable logs = logsProxy.Object;
			Mock<ILoggingService> loggingService = new Mock<ILoggingService>(MockBehavior.Strict);
			loggingService.Setup(x => x.GetLogs()).Returns(logs);
			ILogBaseEnumerable result = loggingService.Object.GetLogs();
			Assert.NotNull(result);
			// use strict equal to do a pointer comparison
			Assert.StrictEqual(logs, result);
		}
	}
}