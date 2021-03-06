using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests
{
	public class StringExtensionsTest
	{
		[Fact]
		public void DeserializeJson()
		{
			string originalString = "{\"Foo\":4, \"Bar\":3}";
			Foobar original = new Foobar
			{
				Foo = 4
			};

			Assert.NotNull(originalString.DeserializeJson<Foobar>());
			Assert.NotEqual(original, originalString.DeserializeJson<Foobar>());
			Assert.Equal(original.Foo, originalString.DeserializeJson<Foobar>().Foo);
			Assert.Equal(original.Bar, originalString.DeserializeJson<Foobar>().Bar);
		}

		[Fact]
		public void GetStreamFromStringWhiteSpace()
		{
			Assert.Empty(" ".ToStream().ToByteArray());
		}

		[Fact]
		public void GetStreamFromString()
		{
			string originalString = "foobar";
			Assert.Equal(originalString, originalString.ToStream().GetString());
		}

		[Fact]
		public void GetBytes()
		{
			string originalString = "foobar";
			byte[] originalBytes = Encoding.UTF8.GetBytes(originalString);
			Assert.Equal(originalBytes, originalString.GetBytes());
		}

		[Fact]
		public void GetString()
		{
			string originalString = "foobar";
			byte[] originalBytes = Encoding.UTF8.GetBytes(originalString);
			Assert.Equal(originalString, StringExtensions.FromBytes(originalBytes));
		}

		[Fact]
		public void GetArbitraryString()
		{
			byte[] originalBytes = { 11, 32, 69 };
			string originalString = Convert.ToBase64String(originalBytes);
			Assert.Equal(originalString, originalBytes.ToArbitraryString());
		}

		[Fact]
		public void Base64Decode()
		{
			string originalString = "foobar";
			string originalStringB64 = Convert.ToBase64String(originalString.GetBytes());
			Assert.Equal(originalString, originalStringB64.Base64Decode());
		}

		[Fact]
		public void Base64Encode()
		{
			string originalString = "foobar";
			string originalStringB64 = Convert.ToBase64String(originalString.GetBytes());
			Assert.Equal(originalStringB64, originalString.Base64Encode());
		}

		[Fact]
		public void GetDateTimeFromUnixTimestampStringTest()
		{
			string epochPlusAMinueString = "60";
			DateTime? epochPlusAMinue = Constants.DateTime.UnixEpoch.AddMinutes(1);

			Assert.Equal(epochPlusAMinue, epochPlusAMinueString.GetDateTimeFromUnixTimestampString());
			Assert.Null(StringExtensions.GetDateTimeFromUnixTimestampString(null));
		}

		[Fact]
		public void GetTimeSpanFromUnixTimestampStringTest()
		{
			string epochPlusAMinueString = "60";
			TimeSpan? epochPlusAMinute = TimeSpan.FromSeconds(60);

			Assert.Equal(epochPlusAMinute, epochPlusAMinueString.GetTimeSpanFromUnixTimestampString());
			Assert.Null(StringExtensions.GetTimeSpanFromUnixTimestampString(null));
		}

		[Fact]
		public void ToSecureStringTest()
		{
			string source = "this is a test string";
			var secure = new SecureString();
			foreach (char c in source)
			{
				secure.AppendChar(c);
			}
			
			Assert.Equal(secure.CalculateHash(), source.ToSecureString().CalculateHash());
		}
	}
}