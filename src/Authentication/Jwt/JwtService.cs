
using System;
using System.Security;
using JWT;
using JWT.Algorithms;
using StandardDot.Abstract.CoreServices;
using StandardDot.CoreExtensions;
using StandardDot.CoreExtensions.Object;
using StandardDot.Enums;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// The base JWT Service Class that will handle JWT authentication
	/// </summary>
	public class JwtService : IDisposable
	{
		/// <param name="args">The <see cref="JwtServiceArgs" /> to use, default null</param>
		public JwtService(JwtServiceArgs args = null)
		{
			_args = args?.Copy() ?? new JwtServiceArgs();
			_secureSecret = args.SecureSecret;
			_secureSecret = _secureSecret ?? args.Secret?.ToSecureString();
			_args.Secret = null;
			_args.SecureSecret?.Dispose();
			_args.SecureSecret = null;
		}

		private SecureString _secureSecret;


		protected virtual JwtServiceArgs _args { get; }
		
		protected virtual IJsonSerializer Serializer => _args.Serializer;
		
		protected virtual IDateTimeProvider Provider => _args.Provider;

		protected virtual IJwtValidator Validator => _args.Validator;

		protected virtual IBase64UrlEncoder UrlEncoder => _args.UrlEncoder;

		protected virtual IJwtDecoder Decoder => _args.Decoder;

		protected virtual IJwtAlgorithm Algorithm => _args.Algorithm;

		protected virtual IJwtEncoder Encoder => _args.Encoder;

		protected virtual ILoggingService LoggingService => _args.LoggingService;

		protected virtual string Secret => _secureSecret?.ToPlainText();

		/// <summary>
		/// Decodes a Jwt Payload with the parameters passed in initially
		/// </summary>
		/// <param name="jwt">The Jwt String</param>
		/// <typeparam name="T">The type of the payload to decode</typeparam>
		/// <exception cref="TokenExpiredException">If the token has expired, logged if logging service available</exception>
		/// <exception cref="SignatureVerificationException">If the token has an invalid signature, logged if logging service available</exception>
		/// <exception cref="Exception">If decoding failed, logged if logging service available</exception>
		/// <returns>The Decoded Payload</returns>
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
		/// Encodes a Jwt Payload using the parameters passed in initially
		/// </summary>
		/// <typeparam name="T">The type of the payload to encode</typeparam>
		/// <param name="payload">The payload that needs to be encoded</param>
		/// <returns>The encoded Jwt Payload</returns>
		public string EncodeJwtPayload<T>(T payload)
		{
			var token = Encoder.Encode(payload, Secret);
			return token;
		}

		public void Dispose()
		{
			_args?.Dispose();
			_secureSecret?.Dispose();
		}
	}
}