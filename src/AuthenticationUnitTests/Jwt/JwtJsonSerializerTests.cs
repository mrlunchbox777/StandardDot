using System;
using System.Collections.Generic;
using Moq;
using StandardDot.Abstract.CoreServices;
using StandardDot.Authentication.Jwt;
using StandardDot.CoreExtensions;
using StandardDot.CoreExtensions.Object;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.Authentication.UnitTests.Jwt
{
	public class JwtJsonSerializerTests
	{
		[Fact]
		public void SettingsTest()
		{
			Mock<ISerializationSettings> settings = new Mock<ISerializationSettings>();
			settings.Setup(x => x.KnownTypes).Returns(new List<Type>{typeof(Foobar)});
			JwtJsonSerializer serializer = new JwtJsonSerializer(settings.Object);
			Foobar foobar = new Foobar
			{
				Bar = 4,
				Foo = 1
			};
			string expected = foobar.SerializeJson();

			Assert.Equal(expected, serializer.Serialize(foobar));
		}

		[Fact]
		public void SerializeTest()
		{
			JwtJsonSerializer serializer = new JwtJsonSerializer();
			Foobar foobar = new Foobar
			{
				Bar = 4,
				Foo = 1
			};
			string expected = foobar.SerializeJson();

			Assert.Equal(expected, serializer.Serialize(foobar));
		}

		[Fact]
		public void DeserializeTest()
		{
			JwtJsonSerializer serializer = new JwtJsonSerializer();
			Foobar foobar = new Foobar
			{
				Bar = 4,
				Foo = 1
			};
			string baseString = foobar.SerializeJson();
			Foobar expected = baseString.DeserializeJson<Foobar>();
			Foobar actual = serializer.Deserialize<Foobar>(baseString);

			Assert.NotEqual(expected, actual);
			Assert.Equal(expected.Bar, actual.Bar);
			Assert.Equal(expected.Foo, actual.Foo);
			Assert.Equal(0, expected.Bar);
		}
	}
}