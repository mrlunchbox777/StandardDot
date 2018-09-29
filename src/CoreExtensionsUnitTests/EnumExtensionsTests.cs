using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

		[Fact]
		public void GetEnumMembersGenericTest()
		{
			Assert.Throws<ArgumentException>(() => 0.GetEnumMembers());
			LogLevel[] allValues = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToArray();
			Assert.NotEmpty(allValues);
			IEnumerable<LogLevel> values = LogLevel.Debug.GetEnumMembers();
			Assert.NotEmpty(values);
			Assert.Equal(allValues, values);
			Assert.NotEqual(HmacIsValidRequestResult.BadNamespace.GetEnumMembers() as IEnumerable,
				values as IEnumerable);
		}

		[Fact]
		public void GetEnumMembersTest()
		{
			Assert.Throws<ArgumentException>(() => typeof(int).GetEnumMembers());
			LogLevel[] allValues = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToArray();
			Assert.NotEmpty(allValues);
			IEnumerable values = typeof(LogLevel).GetEnumMembers();
			Assert.NotEmpty(values);
			Assert.Equal(allValues, values);
			Assert.NotEqual(typeof(HmacIsValidRequestResult).GetEnumMembers() as IEnumerable,
				values as IEnumerable);
		}
	}
}