// using System;
// using JWT;
// using JWT.Algorithms;
// using JWT.Serializers;

// namespace StandardDot.Authentication.Jwt
// {
// 	public class JwtServiceArgs
// 	{
// 		public JwtServiceArgs()
// 			: this(true)
// 		{ }

// 		public JwtServiceArgs(bool useDefaults)
// 		{
// 			if (useDefaults)
// 			{
// 				Secret = Environment.GetEnvironmentVariable("JWT_SECRET");
// 				Serializer = new JsonNetSerializer();
// 				Provider = new UtcDateTimeProvider();
// 				UrlEncoder = new JwtBase64UrlEncoder();
// 				Algorithm = new HMACSHA256Algorithm();
// 				Validator = new JwtValidator(Serializer, Provider); 
// 				Decoder = new JwtDecoder(Serializer, Validator, UrlEncoder);
// 				Encoder = new JwtEncoder(Algorithm, Serializer, UrlEncoder);
// 			}
// 		}

// 		public string Secret { get; set; }
// 		public IJsonSerializer Serializer { get; set; }
// 		public IDateTimeProvider Provider { get; set; }
// 		public IJwtValidator Validator { get; set; } 
// 		public IBase64UrlEncoder UrlEncoder { get; set; }
// 		public IJwtDecoder Decoder { get; set; }
// 		public IJwtAlgorithm Algorithm { get; set; }
// 		public IJwtEncoder Encoder { get; set; }
// 	}
// }