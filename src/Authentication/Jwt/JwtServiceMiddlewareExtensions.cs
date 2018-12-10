using System;
using System.Collections;
using System.Collections.Generic;
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
		/// <param name="updateExpiration">If the middleware should update the JWT expiration, default <c>true</c></param>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService" /> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default <c>"StandardDotJwt"</c></param>
		/// <param name="beforeSubscribers">Subscribers to the after jwt work, before next is called event</param>
		/// <param name="afterSubscribers">Subscribers to the after next is called, before jwt work event</param>
		/// <returns>The application builder with the JWT service added to it</returns>
		public static IApplicationBuilder UseJwtService<T>(this IApplicationBuilder builder, bool updateExpiration = true,
			TimeSpan? jwtExpiration = null, JwtService jwtService = null, string jwtIdentifier = null,
			IEnumerable<JwtServiceMiddleware<T>.AfterJwtWorkBeforeNextArgs> beforeSubscribers = null,
			IEnumerable<JwtServiceMiddleware<T>.AfterNextBeforeJwtWorkArgs> afterSubscribers = null)
			where T : IJwtToken
		{
			builder.UseMiddleware<JwtServiceMiddleware<T>>(updateExpiration, jwtExpiration, jwtIdentifier, jwtService,
				beforeSubscribers, afterSubscribers);
			return builder;
		}
	}
}