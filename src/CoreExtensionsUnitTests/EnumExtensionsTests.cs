using System;
using StandardDot.Enums;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests
{
	public class EnumExtensionsTests
	{
		[Fact]
		public void TryParseEnumSafeTest()
		{
			Assert.Throws<ArgumentException>(() => "0".TryParseEnumSafe<int>());
			Assert.Null("test".TryParseEnumSafe<LogLevel>());
			Assert.Null("".TryParseEnumSafe<LogLevel>());
			Assert.Null(" ".TryParseEnumSafe<LogLevel>());
			Assert.Equal(LogLevel.Debug, "Debug".TryParseEnumSafe<LogLevel>());
			Assert.Equal(LogLevel.Debug, "debug".TryParseEnumSafe<LogLevel>());
			Assert.Equal(LogLevel.Debug, " debug".TryParseEnumSafe<LogLevel>());
			Assert.Equal(LogLevel.Debug, "debug ".TryParseEnumSafe<LogLevel>());
			Assert.Equal(LogLevel.Debug, " debug ".TryParseEnumSafe<LogLevel>());
		}
	}
}