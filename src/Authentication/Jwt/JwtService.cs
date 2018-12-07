
using System;
using System.Security;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.CoreExtensions;
using StandardDot.CoreExtensions.Object;
using StandardDot.Enums;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// The base JWT Service Class that will handle JWT authentication
	/// </summary>
	public class JwtService
	{
		public JwtService(JwtServiceArgs args = null)
		{
			args = args?.Copy() ?? new JwtServiceArgs();
			_secureSecret = args.SecureSecret;
			_secret = _secureSecret?.ToPlainText() ?? args.Secret;
		}

		private SecureString _secureSecret;

		private string _secret;


		protected virtual JwtServiceArgs _args { get; }
		
		protected virtual IJsonSerializer Serializer => _args.Serializer;
		
		protected virtual IDateTimeProvider Provider => _args.Provider;

		protected virtual IJwtValidator Validator => _args.Validator;

		protected virtual IBase64UrlEncoder UrlEncoder => _args.UrlEncoder;

		protected virtual IJwtDecoder Decoder => _args.Decoder;

		protected virtual IJwtAlgorithm Algorithm => _args.Algorithm;

		protected virtual IJwtEncoder Encoder => _args.Encoder;
		
		protected virtual ICachingService CachingService => _args.CachingService;

		protected virtual ILoggingService LoggingService => _args.LoggingService;

		protected virtual string Secret => _secret;

		/// <summary>
		/// 
		/// </summary>
		/// <param name=""></param>
		/// <returns></returns>
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
				throw;
			}
			catch (SignatureVerificationException ex)
			{
				LoggingService?.LogException(ex, "Token has invalid signature", LogLevel.Info);
				throw;
			}
			catch (Exception ex)
			{
				LoggingService?.LogException(ex, "Decoding Jwt threw an exception", LogLevel.Error);
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name=""></param>
		/// <returns></returns>
		public string EncodeJwtPayload<T>(T payload)
		{
			var token = Encoder.Encode(payload, Secret);
			return token;
		}

		public void RegisterJwt<T>(T jwt)
		{
			if (CachingService != null)
			{

			}
		}
	}
}