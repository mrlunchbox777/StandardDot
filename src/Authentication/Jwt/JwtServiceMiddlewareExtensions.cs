using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using StandardDot.Authentication.Jwt.EventArgs;

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
		/// <param name="builder">The application builder to add the middleware to</param>
		/// <param name="updateExpiration">If the middleware should update the JWT expiration, default <c>true</c></param>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService" /> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default <c>"StandardDotJwt"</c></param>
		/// <param name="beforeSubscribers">Subscribers to the after jwt work, before next is called event</param>
		/// <param name="afterSubscribers">Subscribers to the after next is called, before jwt work event</param>
		/// <returns>The application builder with the JWT service added to it</returns>
		public static IApplicationBuilder UseJwtService<T>(this IApplicationBuilder builder, bool updateExpiration = true,
			TimeSpan? jwtExpiration = null, JwtService jwtService = null, string jwtIdentifier = null,
			IEnumerable<Func<JwtServiceMiddleware<T>, JwtEventArgs<T>, Task>> beforeSubscribers = null,
			IEnumerable<Func<JwtServiceMiddleware<T>, AfterWorkJwtEventArgs<T>, Task>> afterSubscribers = null)
			where T : IJwtToken
		{
			builder.UseMiddleware<JwtServiceMiddleware<T>>(updateExpiration, jwtExpiration, jwtIdentifier, jwtService,
				beforeSubscribers, afterSubscribers);
			return builder;
		}

		/// <summary>
		/// Registers a JwtService to the IOC
		/// </summary>
		/// <typeparam name="T">The type for the JWT</typeparam>
		/// <param name="beforeSubscriber">A Subscriber to the after jwt work, before next is called event</param>
		/// <param name="updateExpiration">If the middleware should update the JWT expiration, default <c>true</c></param>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService" /> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default <c>"StandardDotJwt"</c></param>
		/// <param name="afterSubscribers">Subscribers to the after next is called, before jwt work event</param>
		public static IApplicationBuilder UseJwtService<T>(this IApplicationBuilder builder,
			Func<JwtServiceMiddleware<T>, JwtEventArgs<T>, Task> beforeSubscriber,
			bool updateExpiration = true, TimeSpan? jwtExpiration = null,
			string jwtIdentifier = null, JwtService jwtService = null,
			IEnumerable<Func<JwtServiceMiddleware<T>, AfterWorkJwtEventArgs<T>, Task>> afterSubscribers = null)
			where T : IJwtToken
		{
			return UseJwtService(builder, updateExpiration, jwtExpiration, jwtService, jwtIdentifier, new [] {beforeSubscriber}, afterSubscribers);
		}
		
		/// <summary>
		/// Registers a JwtService to the IOC
		/// </summary>
		/// <typeparam name="T">The type for the JWT</typeparam>
		/// <param name="afterSubscriber">Subscribers to the after next is called, before jwt work event</param>
		/// <param name="updateExpiration">If the middleware should update the JWT expiration, default <c>true</c></param>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService" /> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default <c>"StandardDotJwt"</c></param>
		/// <param name="beforeSubscribers">A Subscriber to the after jwt work, before next is called event</param>
		public static IApplicationBuilder UseJwtService<T>(this IApplicationBuilder builder,
			Func<JwtServiceMiddleware<T>, AfterWorkJwtEventArgs<T>, Task> afterSubscriber,
			bool updateExpiration = true, TimeSpan? jwtExpiration = null,
			string jwtIdentifier = null, JwtService jwtService = null,
			IEnumerable<Func<JwtServiceMiddleware<T>, JwtEventArgs<T>, Task>> beforeSubscribers = null)
			where T : IJwtToken
		{
			return UseJwtService(builder, updateExpiration, jwtExpiration, jwtService, jwtIdentifier, beforeSubscribers, new [] {afterSubscriber});
		}
		
		/// <summary>
		/// Registers a JwtService to the IOC
		/// </summary>
		/// <typeparam name="T">The type for the JWT</typeparam>
		/// <param name="beforeSubscriber">A Subscriber to the after jwt work, before next is called event</param>
		/// <param name="afterSubscriber">A Subscriber to the after next is called, before jwt work event</param>
		/// <param name="updateExpiration">If the middleware should update the JWT expiration, default <c>true</c></param>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService" /> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default <c>"StandardDotJwt"</c></param>
		public static IApplicationBuilder UseJwtService<T>(this IApplicationBuilder builder,
			Func<JwtServiceMiddleware<T>, JwtEventArgs<T>, Task> beforeSubscriber,
			Func<JwtServiceMiddleware<T>, AfterWorkJwtEventArgs<T>, Task> afterSubscriber,
			bool updateExpiration = true, TimeSpan? jwtExpiration = null,
			string jwtIdentifier = null, JwtService jwtService = null)
			where T : IJwtToken
		{
			return UseJwtService(builder, updateExpiration, jwtExpiration, jwtService, jwtIdentifier, new [] {beforeSubscriber}, new [] {afterSubscriber});
		}
	}
}