using System;
using System.Security;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using StandardDot.Abstract.CoreServices;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// Provides the arguments for the Jwt Service including a set of good defaults
	/// </summary>
	public class JwtServiceArgs : IDisposable
	{
		/// <summary>
		/// Create the default set of JwtServiceArgs
		/// </summary>
		/// <param name="loggingService">The logging service to log errors with, default null</param>
		/// <param name="secureSecret">The secure secret used to encode JWTs, default null</param>
		public JwtServiceArgs(ILoggingService loggingService = null, SecureString secureSecret = null)
			: this(true, loggingService, secureSecret)
		{ }

		/// <summary>
		/// Create a new JwtServiceArgs with optionally default arguments
		/// </summary>
		/// <param name="useDefaults">If defaults should be used when creating the arguments</param>
		/// <param name="loggingService">The logging service to log errors with, default null</param>
		/// <param name="secureSecret">The secure secret used to encode JWTs, default null</param>
		public JwtServiceArgs(bool useDefaults, ILoggingService loggingService = null, SecureString secureSecret = null)
		{
			if (useDefaults)
			{
				Serializer = new JsonNetSerializer();
				Provider = new UtcDateTimeProvider();
				UrlEncoder = new JwtBase64UrlEncoder();
				Algorithm = new HMACSHA256Algorithm();
				Secret = Environment.GetEnvironmentVariable("JWT_SECRET");
				
				Validator = new JwtValidator(Serializer, Provider);
				Decoder = new JwtDecoder(Serializer, Validator, UrlEncoder);
				Encoder = new JwtEncoder(Algorithm, Serializer, UrlEncoder);
			}
		}

		/// <summary>The Json serialization service used to handle JWTs, default <see cref="JsonNetSerializer" /></summary>
		public IJsonSerializer Serializer { get; set; }

		/// <summary>The provider for DateTimes, default <see cref="UtcDateTimeProvider" /></summary>
		public IDateTimeProvider Provider { get; set; }

		/// <summary>The validation service for JWTs, default <see cref="JwtValidator" /></summary>
		public IJwtValidator Validator { get; set; }

		/// <summary>The Base64 Encoding Service for JWTs, default <see cref="JwtBase64UrlEncoder" /></summary>
		public IBase64UrlEncoder UrlEncoder { get; set; }

		/// <summary>The Decoding Service for JWTs, default <see cref="JwtDecoder" /></summary>
		public IJwtDecoder Decoder { get; set; }

		/// <summary>The Encryption Algorithm for JWTs, default <see cref="HMACSHA256Algorithm" /></summary>
		public IJwtAlgorithm Algorithm { get; set; }

		/// <summary>The Encoding Service for JWTs, default <see cref="JwtEncoder" /></summary>
		public IJwtEncoder Encoder { get; set; }

		/// <summary>The plain text secret used to encrypt the JWT, default <c>Environment.GetEnvironmentVariable("JWT_SECRET")</c></summary>
		public string Secret { get; set; }

		/// <summary>The logging service for errors, default null - no logging</summary>
		public ILoggingService LoggingService { get; set; }

		/// <summary>The secure secret used to encrypt the JWT</summary>
		public SecureString SecureSecret { get; set; }

		public void Dispose()
		{
			SecureSecret?.Dispose();
		}
	}
}