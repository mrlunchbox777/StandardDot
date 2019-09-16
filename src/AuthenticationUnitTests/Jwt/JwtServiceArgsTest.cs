using System;
using System.Reflection;
using System.Security;
using JWT;
using JWT.Algorithms;
using Moq;
using StandardDot.Abstract.CoreServices;
using StandardDot.Authentication.Jwt;
using StandardDot.CoreExtensions;
using StandardDot.CoreExtensions.Object;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.Authentication.UnitTests.Jwt
{
	public class JwtServiceArgsTests
	{
		private readonly string _secret = "Machine or mannequin";

		[Fact]
		public void DefaultPropertiesTest()
		{
			try
			{
				Environment.SetEnvironmentVariable("JWT_SECRET", _secret);
				using (JwtServiceArgs args = new JwtServiceArgs())
				{
					Assert.Null(args.SecureSecret);
					Assert.Equal(_secret, args.Secret);
					Assert.NotNull(args.Serializer);
					Assert.NotNull(args.Provider);
					Assert.NotNull(args.Validator);
					Assert.NotNull(args.UrlEncoder);
					Assert.NotNull(args.Decoder);
					Assert.NotNull(args.Algorithm);
					Assert.NotNull(args.Encoder);
					Assert.Null(args.LoggingService);
				}
			}
			finally
			{
				Environment.SetEnvironmentVariable("JWT_SECRET", null);
			}
		}

		[Fact]
		public void ConstructorPropertiesTest()
		{
			Mock<ILoggingService> mLoggingService = new Mock<ILoggingService>();
			ILoggingService loggingService = mLoggingService.Object;
			using (SecureString secret = _secret.ToSecureString())
			using (JwtServiceArgs args = new JwtServiceArgs(false, loggingService, secret))
			{
				Assert.Null(args.Secret);
				Assert.Equal(secret, args.SecureSecret);
				Assert.Equal(_secret, args.SecureSecret.ToPlainText());
				Assert.Null(args.Serializer);
				Assert.Null(args.Provider);
				Assert.Null(args.Validator);
				Assert.Null(args.UrlEncoder);
				Assert.Null(args.Decoder);
				Assert.Null(args.Algorithm);
				Assert.Null(args.Encoder);
				Assert.Equal(loggingService, args.LoggingService);
			}
		}

		[Fact]
		public void InitializerPropertiesTest()
		{
			Mock<IJsonSerializer> mSerializer = new Mock<IJsonSerializer>();
			IJsonSerializer serializer = mSerializer.Object;
			Mock<IDateTimeProvider> mProvider = new Mock<IDateTimeProvider>();
			IDateTimeProvider provider = mProvider.Object;
			Mock<IJwtValidator> mValidator = new Mock<IJwtValidator>();
			IJwtValidator validator = mValidator.Object;
			Mock<IBase64UrlEncoder> mUrlEncoder = new Mock<IBase64UrlEncoder>();
			IBase64UrlEncoder urlEncoder = mUrlEncoder.Object;
			Mock<IJwtDecoder> mDecoder = new Mock<IJwtDecoder>();
			IJwtDecoder decoder = mDecoder.Object;
			Mock<IJwtAlgorithm> mAlgorithm = new Mock<IJwtAlgorithm>();
			IJwtAlgorithm algorithm = mAlgorithm.Object;
			Mock<IJwtEncoder> mEncoder = new Mock<IJwtEncoder>();
			IJwtEncoder encoder = mEncoder.Object;
			Mock<ILoggingService> mLoggingService = new Mock<ILoggingService>();
			ILoggingService loggingService = mLoggingService.Object;
			using (SecureString secret = _secret.ToSecureString())
			using (JwtServiceArgs args = new JwtServiceArgs(false, null, null)
			{
				Secret = _secret,
				SecureSecret = secret,
				Serializer = serializer,
				Provider = provider,
				Validator = validator,
				UrlEncoder = urlEncoder,
				Decoder = decoder,
				Algorithm = algorithm,
				Encoder = encoder,
				LoggingService = loggingService
			})
			{
				Assert.Equal(_secret, args.Secret);
				Assert.Equal(secret, args.SecureSecret);
				Assert.Equal(_secret, args.SecureSecret.ToPlainText());
				Assert.Equal(serializer, args.Serializer);
				Assert.Equal(provider, args.Provider);
				Assert.Equal(validator, args.Validator);
				Assert.Equal(urlEncoder, args.UrlEncoder);
				Assert.Equal(decoder, args.Decoder);
				Assert.Equal(algorithm, args.Algorithm);
				Assert.Equal(encoder, args.Encoder);
				Assert.Equal(loggingService, args.LoggingService);
			}
		}
	}
}