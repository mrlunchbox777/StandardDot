
using System;
using System.Security;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using StandardDot.Abstract.CoreServices;
using StandardDot.Enums;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// The base JWT Service Class that will handle JWT authentication
	/// </summary>
	public class JwtService
	{
		/// <param name="secret">The secret used to encrypt the JWT</param>
		/// <param name="serializer">The Json serialization service used to handle JWTs</param>
		/// <param name="provider">The provider for DateTimes</param>
		/// <param name="validator">The validation service for JWTs</param>
		/// <param name="urlEncoder">The Base64 Encoding Service for JWTs</param>
		/// <param name="decoder">The Decoding Service for JWTs</param>
		/// <param name="algorithm">The Encryption Algorith for JWTs</param>
		/// <param name="encoder">The Encoding Service for JWTs</param>
		/// <param name="loggingService">The logging service for errors, default null - no logging</param>
		public JwtService(IJsonSerializer serializer, IDateTimeProvider provider, IJwtValidator validator,
			IBase64UrlEncoder urlEncoder, IJwtDecoder decoder, IJwtAlgorithm algorithm, IJwtEncoder encoder,
			ILoggingService loggingService = null, string secret = null, SecureString secureSecret = null)
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
			_secureSecret = secureSecret;
		}

		private SecureString _secureSecret;

		private string _secret;

		protected virtual string Secret => _secret ?? _secureSecret.;
		
		protected virtual IJsonSerializer Serializer { get; }
		
		protected virtual IDateTimeProvider Provider { get; }
		
		protected virtual IJwtValidator Validator { get; } 
		
		protected virtual IBase64UrlEncoder UrlEncoder { get; }
		
		protected virtual IJwtDecoder Decoder { get; }
		
		protected virtual IJwtAlgorithm Algorithm { get; }
		
		protected virtual IJwtEncoder Encoder { get; }
		
		protected virtual ILoggingService LoggingService { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name=""></param>
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
			catch (Exception ex)
			{
				LoggingService?.LogException(ex, "Decoding Jwt threw an exception", LogLevel.Error);
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