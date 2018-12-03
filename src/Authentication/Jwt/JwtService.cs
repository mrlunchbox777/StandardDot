
using System;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using StandardDot.Abstract.CoreServices;
using StandardDot.Enums;

namespace StandardDot.Authentication.Jwt
{
	public class JwtService
	{
		public JwtService(string secret, IJsonSerializer serializer, IDateTimeProvider provider, IJwtValidator validator,
			IBase64UrlEncoder urlEncoder, IJwtDecoder decoder, IJwtAlgorithm algorithm, IJwtEncoder encoder,
			ILoggingService loggingService = null)
		{
			Serializer = serializer;
			Provider = provider;
			Validator = validator;
			UrlEncoder = urlEncoder;
			Decoder = decoder;
			Algorithm = algorithm;
			Encoder = encoder;
			LoggingService = loggingService;
			_secret = secret;
		}

		private string _secret;
		private string Secret => _secret;
		private IJsonSerializer Serializer { get; set; }
		private IDateTimeProvider Provider { get; set; }
		private IJwtValidator Validator { get; set; } 
		private IBase64UrlEncoder UrlEncoder { get; set; }
		private IJwtDecoder Decoder { get; set; }
		private IJwtAlgorithm Algorithm { get; set; }
		private IJwtEncoder Encoder { get; set; }
		private ILoggingService LoggingService { get; set; }

		public T DecodeJwtPayload<T>(string jwt)
		{
			try
			{
				T payload = Decoder.DecodeToObject<T>(jwt, Secret, verify: true);
				return payload;
			}
			catch (TokenExpiredException ex)
			{
				LoggingService?.LogException(ex, "Token has expired", LogLevel.Info);
			}
			catch (SignatureVerificationException ex)
			{
				LoggingService?.LogException(ex, "Token has invalid signature", LogLevel.Info);
			}
			return default(T);
		}

		public string EncodeJwtPayload<T>(T payload)
		{
			var token = Encoder.Encode(payload, Secret);
			return token;
		}
	}
}