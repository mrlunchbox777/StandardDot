using System;
using Microsoft.AspNetCore.Builder;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// Extensions for IApplicationBuilder that registers a JwtService to the IOC
	/// </summary>
	public static class JwtServiceMiddlewareExtensions
	{
		/// <summary>
		/// Registers a JwtService to the IOC
		/// </summary>
		/// <typeparam name="T">The type for the JWT</typeparam>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService"> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default StandardDotJwt</param>
		/// <returns>The application builder with the JWT service added to it</returns>
		public static IApplicationBuilder UseJwtService<T>(this IApplicationBuilder builder,
			TimeSpan? jwtExpiration = null, JwtService jwtService = null, string jwtIdentifier = null)
			where T : IJwtToken
		{
			return builder.UseMiddleware<JwtServiceMiddleware<T>>(jwtExpiration, jwtService, jwtIdentifier);
		}
	}
}