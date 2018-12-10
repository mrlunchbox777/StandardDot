using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// The middleware for a <see cref="JwtService" />.
	/// It pulls the Jwt from the cookies/headers with the <c>jwtIdentifier</c>
	/// and puts it into the <c>HttpContext.Items</c> using the same <c>jwtIdentifier</c>
	/// </summary>
	/// <typeparam name="T">The type for the JWT</typeparam>
	public class JwtServiceMiddleware<T>
		where T : IJwtToken
	{
		/// <param name="next">The next middleware to run</param>
		/// <param name="updateExpiration">If the middleware should update the JWT expiration, default <c>true</c></param>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService" /> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default <c>"StandardDotJwt"</c></param>
		/// <param name="beforeSubscribers">Subscribers to the after jwt work, before next is called event</param>
		/// <param name="afterSubscribers">Subscribers to the after next is called, before jwt work event</param>
		public JwtServiceMiddleware(RequestDelegate next, bool updateExpiration = true, TimeSpan? jwtExpiration = null,
			string jwtIdentifier = null, JwtService jwtService = null,
			IEnumerable<AfterJwtWorkBeforeNextArgs> beforeSubscribers = null, IEnumerable<AfterNextBeforeJwtWorkArgs> afterSubscribers = null)
		{
			_next = next;
			_updateExpiration = updateExpiration;
			_jwtIdentifier = jwtIdentifier ?? "StandardDotJwt";
			_jwtService = jwtService ?? new JwtService();
			_jwtExpiration = jwtExpiration ?? new TimeSpan(1, 0, 0);
			if (beforeSubscribers != null)
			{
				foreach(AfterJwtWorkBeforeNextArgs beforeSubscriber in beforeSubscribers)
				{
					AfterJwtWorkBeforeNext += beforeSubscriber;
				}
			}
			if (afterSubscribers != null)
			{
				foreach(AfterNextBeforeJwtWorkArgs afterSubscriber in afterSubscribers)
				{
					AfterNextBeforeJwtWork += afterSubscriber;
				}
			}
		}

		private readonly TimeSpan _jwtExpiration;

		protected readonly RequestDelegate _next;
		
		protected readonly string _jwtIdentifier;
		
		protected readonly JwtService _jwtService;
		
		protected readonly bool _updateExpiration;

		/// <summary>
		/// Runs the middleware, uses cookie over header if able
		/// </summary>
		/// <param name="context">The context for the current request</param>
		/// <returns>A Task for when this request has completed</returns>
		public virtual async Task InvokeAsync(HttpContext context)
		{
			string cookie = GetCookieFromContext(context);
			string header = GetHeaderFromContext(context);
			T token = GetTokenFromCookieOrHeader(cookie, header);
			context.Items.Add(_jwtIdentifier, token);
			await AfterJwtWorkBeforeNext(this, context, cookie, header, token);
			// Call the next delegate/middleware in the pipeline
			await _next(context);
			// do after context work such as updating the jwt as shown below
			string newCookie = GetCookieFromContext(context);
			string newHeader = GetHeaderFromContext(context);
			T newToken = GetTokenFromCookieOrHeader(newCookie, newHeader);
			await AfterNextBeforeJwtWork(this, context, cookie, header, token, newCookie, newHeader, newToken);
			if (_updateExpiration && !context.Response.HasStarted)
			{
				newToken.Expiration = DateTime.UtcNow.Add(_jwtExpiration);
				if (!string.IsNullOrWhiteSpace(newCookie))
				{
					context.Response.Cookies.Append(_jwtIdentifier, _jwtService.EncodeJwtPayload(newToken),
						new CookieOptions {Expires = newToken.Expiration});
				}
				if (!string.IsNullOrWhiteSpace(newHeader))
				{
					context.Response.Headers.Append(_jwtIdentifier, _jwtService.EncodeJwtPayload(newToken));
				}
			}
		}

		protected virtual T GetTokenFromCookieOrHeader(string cookie, string header)
		{
			if (string.IsNullOrWhiteSpace(cookie) && string.IsNullOrWhiteSpace(header))
			{
				return default(T);
			}

			return _jwtService.DecodeJwtPayload<T>(!string.IsNullOrWhiteSpace(cookie)
				? cookie
				: header);
		}

		protected virtual string GetCookieFromContext(HttpContext context)
		{
			return context?.Request?.Cookies?[_jwtIdentifier];
		}

		protected virtual string GetHeaderFromContext(HttpContext context)
		{
			return context?.Request?.Headers?[_jwtIdentifier];
		}

		/// <summary>
		/// A method that will be run after jwt work and before the next call is made
		/// </summary>
		/// <param name="sender">The <see cref="JwtServiceMiddleware" /> that raised the event</param>
		/// <param name="context">The context for the current request</param>
		/// <param name="cookie">The string for the cookie pulled from the request, possibly null</param>
		/// <param name="header">The string for the header pulled from the request, possibly null</param>
		/// <param name="token">The token pulled from the request, possibly null</param>
		/// <returns>A Task for when this subscriber has completed</returns>
		public delegate Task AfterJwtWorkBeforeNextArgs(JwtServiceMiddleware<T> sender, HttpContext context,
			string cookie, string header, T token);

		/// <summary>
		/// An event that will be raised after jwt work is done, but before next is called
		/// </summary>
		protected event AfterJwtWorkBeforeNextArgs AfterJwtWorkBeforeNext;

		/// <summary>
		/// A method that will be run after the next call is made and before jwt work
		/// </summary>
		/// <param name="sender">The <see cref="JwtServiceMiddleware" /> that raised the event</param>
		/// <param name="context">The context for the current request</param>
		/// <param name="cookie">The string for the cookie pulled from the request before the next call, possibly null</param>
		/// <param name="header">The string for the header pulled from the request before the next call, possibly null</param>
		/// <param name="originalToken">The token pulled from the request before the next call, possibly null</param>
		/// <param name="cookie">The string for the cookie pulled from the request, possibly null</param>
		/// <param name="header">The string for the header pulled from the request, possibly null</param>
		/// <param name="token">The token pulled from the request, possibly null</param>
		/// <returns>A Task for when this subscriber has completed</returns>
		public delegate Task AfterNextBeforeJwtWorkArgs(JwtServiceMiddleware<T> sender, HttpContext context,
			string originalCookie, string originalHeader, T originalToken, string cookie, string header, T token);

		/// <summary>
		/// An event that will be raised after next is calle, but before jwt work is done
		/// </summary>
		protected event AfterNextBeforeJwtWorkArgs AfterNextBeforeJwtWork;
	}
}