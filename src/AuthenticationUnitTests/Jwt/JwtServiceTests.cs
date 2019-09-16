using System;
using System.Reflection;
using System.Security;
using Moq;
using StandardDot.Authentication.Jwt;
using StandardDot.CoreExtensions;
using StandardDot.CoreExtensions.Object;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.Authentication.UnitTests.Jwt
{
	public class JwtServiceTests
	{
		private readonly string _secret = "I've got a secret";

		[Fact]
		public void SecureStringSecretTest()
		{
			using (SecureString secret = _secret.ToSecureString())
			{
				using (JwtServiceArgs args = new JwtServiceArgs
				{
					SecureSecret = secret
				})
				using (JwtService service = new JwtService(args))
				{
					PropertyInfo prop = typeof(JwtService)
						.GetProperty("Secret", BindingFlags.NonPublic | BindingFlags.Instance);
					Assert.Equal(_secret, prop.GetValue(service));
				}
			}
		}

		[Fact]
		public void PlainTextSecretTest()
		{
			using (JwtServiceArgs args = new JwtServiceArgs
			{
				Secret = _secret
			})
			using (JwtService service = new JwtService(args))
			{
				PropertyInfo prop = typeof(JwtService)
					.GetProperty("Secret", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(_secret, prop.GetValue(service));
			}
		}

		[Fact]
		public void EnvironmentalVariableSecretTest()
		{
			try
			{
				Environment.SetEnvironmentVariable("JWT_SECRET", _secret);
				using (JwtServiceArgs args = new JwtServiceArgs())
				using (JwtService service = new JwtService(args))
				{
					PropertyInfo prop = typeof(JwtService)
						.GetProperty("Secret", BindingFlags.NonPublic | BindingFlags.Instance);
					Assert.Equal(_secret, prop.GetValue(service));
				}
			}
			finally
			{
				Environment.SetEnvironmentVariable("JWT_SECRET", null);
			}
		}

		[Fact]
		public void DefaultsSecretTest()
		{
			try
			{
				Environment.SetEnvironmentVariable("JWT_SECRET", _secret);
				using (JwtService service = new JwtService())
				{
					PropertyInfo prop = typeof(JwtService)
						.GetProperty("Secret", BindingFlags.NonPublic | BindingFlags.Instance);
					Assert.Equal(_secret, prop.GetValue(service));
				}
			}
			finally
			{
				Environment.SetEnvironmentVariable("JWT_SECRET", null);
			}
		}
	
		[Fact]
		public void PropertiesTest()
		{
			using (JwtServiceArgs args = new JwtServiceArgs
			{
				Secret = _secret
			})
			using (JwtService service = new JwtService(args))
			{
				PropertyInfo prop = typeof(JwtService)
					.GetProperty("Secret", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(_secret, prop.GetValue(service));
				
				FieldInfo field = typeof(JwtService)
					.GetField("_secureSecret", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(_secret, ((SecureString)field.GetValue(service)).ToPlainText());
				
				prop = typeof(JwtService)
					.GetProperty("_args", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(args, prop.GetValue(service));
				Assert.NotNull(prop.GetValue(service));
				
				prop = typeof(JwtService)
					.GetProperty("Serializer", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(args.Serializer, prop.GetValue(service));
				Assert.NotNull(prop.GetValue(service));
				
				prop = typeof(JwtService)
					.GetProperty("Provider", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(args.Provider, prop.GetValue(service));
				Assert.NotNull(prop.GetValue(service));

				prop = typeof(JwtService)
					.GetProperty("Validator", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(args.Validator, prop.GetValue(service));
				Assert.NotNull(prop.GetValue(service));
				
				prop = typeof(JwtService)
					.GetProperty("UrlEncoder", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(args.UrlEncoder, prop.GetValue(service));
				Assert.NotNull(prop.GetValue(service));
				
				prop = typeof(JwtService)
					.GetProperty("Decoder", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(args.Decoder, prop.GetValue(service));
				Assert.NotNull(prop.GetValue(service));
				
				prop = typeof(JwtService)
					.GetProperty("Algorithm", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(args.Algorithm, prop.GetValue(service));
				Assert.NotNull(prop.GetValue(service));
				
				prop = typeof(JwtService)
					.GetProperty("Encoder", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.Equal(args.Encoder, prop.GetValue(service));
				Assert.NotNull(prop.GetValue(service));

				prop = typeof(JwtService)
					.GetProperty("LoggingService", BindingFlags.NonPublic | BindingFlags.Instance);
				// in this case both will be null
				Assert.Equal(args.LoggingService, prop.GetValue(service));
				Assert.Null(prop.GetValue(service));
			}
		}

		[Fact]
		public void EncodeJwtPayloadTest()
		{
			using (JwtServiceArgs args = new JwtServiceArgs
			{
				Secret = _secret
			})
			using (JwtService service = new JwtService(args))
			{
				Foobar foobar = new Foobar
				{
					Bar = 4,
					Foo = 1
				};
				string expected = args.Encoder.Encode(foobar, _secret);

				Assert.Equal(expected, service.EncodeJwtPayload(foobar));
			}
		}

		[Fact]
		public void DecodeJwtPayloadTest()
		{
			using (JwtServiceArgs args = new JwtServiceArgs
			{
				Secret = _secret
			})
			using (JwtService service = new JwtService(args))
			{
				Foobar foobar = new Foobar
				{
					Bar = 4,
					Foo = 1
				};
				string baseString = args.Encoder.Encode(foobar, _secret);
				Foobar expected = args.Decoder.DecodeToObject<Foobar>(baseString, _secret, true);
				Foobar actual = service.DecodeJwtPayload<Foobar>(baseString);

				Assert.NotEqual(expected, actual);
				Assert.Equal(expected.Bar, actual.Bar);
				Assert.Equal(expected.Foo, actual.Foo);
				Assert.Equal(0, expected.Bar);
			}
		}
	}
}