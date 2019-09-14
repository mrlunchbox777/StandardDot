using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StandardDot.Authentication.Jwt.EventArgs;
using StandardDot.Core.Event;

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
			IEnumerable<Func<JwtServiceMiddleware<T>, JwtEventArgs<T>, Task>> beforeSubscribers = null,
			IEnumerable<Func<JwtServiceMiddleware<T>, AfterWorkJwtEventArgs<T>, Task>> afterSubscribers = null)
		{
			_next = next;
			_updateExpiration = updateExpiration;
			_jwtIdentifier = jwtIdentifier ?? "StandardDotJwt";
			_jwtService = jwtService ?? new JwtService();
			_jwtExpiration = jwtExpiration ?? new TimeSpan(1, 0, 0);
			if (beforeSubscribers != null)
			{
				foreach(Func<JwtServiceMiddleware<T>, JwtEventArgs<T>, Task> beforeSubscriber in beforeSubscribers)
				{
					AfterJwtWorkBeforeNext += beforeSubscriber;
				}
			}
			if (afterSubscribers != null)
			{
				foreach(Func<JwtServiceMiddleware<T>, AfterWorkJwtEventArgs<T>, Task> afterSubscriber in afterSubscribers)
				{
					AfterNextBeforeJwtWork += afterSubscriber;
				}
			}
		}

		/// <param name="next">The next middleware to run</param>
		/// <param name="beforeSubscriber">A Subscriber to the after jwt work, before next is called event</param>
		/// <param name="updateExpiration">If the middleware should update the JWT expiration, default <c>true</c></param>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService" /> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default <c>"StandardDotJwt"</c></param>
		/// <param name="afterSubscribers">Subscribers to the after next is called, before jwt work event</param>
		public JwtServiceMiddleware(RequestDelegate next,
			Func<JwtServiceMiddleware<T>, JwtEventArgs<T>, Task> beforeSubscriber,
			bool updateExpiration = true, TimeSpan? jwtExpiration = null,
			string jwtIdentifier = null, JwtService jwtService = null,
			IEnumerable<Func<JwtServiceMiddleware<T>, AfterWorkJwtEventArgs<T>, Task>> afterSubscribers = null)
			: this(next, updateExpiration, jwtExpiration, jwtIdentifier, jwtService, new [] {beforeSubscriber}, afterSubscribers)
		{ }
		
		/// <param name="next">The next middleware to run</param>
		/// <param name="afterSubscriber">Subscribers to the after next is called, before jwt work event</param>
		/// <param name="updateExpiration">If the middleware should update the JWT expiration, default <c>true</c></param>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService" /> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default <c>"StandardDotJwt"</c></param>
		/// <param name="beforeSubscribers">A Subscriber to the after jwt work, before next is called event</param>
		public JwtServiceMiddleware(RequestDelegate next,
			Func<JwtServiceMiddleware<T>, AfterWorkJwtEventArgs<T>, Task> afterSubscriber,
			bool updateExpiration = true, TimeSpan? jwtExpiration = null,
			string jwtIdentifier = null, JwtService jwtService = null,
			IEnumerable<Func<JwtServiceMiddleware<T>, JwtEventArgs<T>, Task>> beforeSubscribers = null)
			: this(next, updateExpiration, jwtExpiration, jwtIdentifier, jwtService, beforeSubscribers, new [] {afterSubscriber})
		{ }
		
		/// <param name="next">The next middleware to run</param>
		/// <param name="beforeSubscriber">A Subscriber to the after jwt work, before next is called event</param>
		/// <param name="afterSubscriber">A Subscriber to the after next is called, before jwt work event</param>
		/// <param name="updateExpiration">If the middleware should update the JWT expiration, default <c>true</c></param>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService" /> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default <c>"StandardDotJwt"</c></param>
		public JwtServiceMiddleware(RequestDelegate next,
			Func<JwtServiceMiddleware<T>, JwtEventArgs<T>, Task> beforeSubscriber,
			Func<JwtServiceMiddleware<T>, AfterWorkJwtEventArgs<T>, Task> afterSubscriber,
			bool updateExpiration = true, TimeSpan? jwtExpiration = null,
			string jwtIdentifier = null, JwtService jwtService = null)
			: this(next, updateExpiration, jwtExpiration, jwtIdentifier, jwtService, new [] {beforeSubscriber}, new [] {afterSubscriber})
		{ }

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
			await AfterJwtWorkBeforeNext.Raise(this, new JwtEventArgs<T>
			{
				Context = context,
				Cookie = cookie,
				Header = header,
				Token = token
			});
			// Call the next delegate/middleware in the pipeline
			await _next(context);
			// do after context work such as updating the jwt as shown below
			string newCookie = GetCookieFromContext(context);
			string newHeader = GetHeaderFromContext(context);
			T newToken = GetTokenFromCookieOrHeader(newCookie, newHeader);
			await AfterNextBeforeJwtWork.Raise(this, new AfterWorkJwtEventArgs<T>
				{
					BeforeArgs = new JwtEventArgs<T>
					{
						Context = context,
						Cookie = cookie,
						Header = header,
						Token = token
					},
					AfterArgs = new JwtEventArgs<T>
					{
						Context = context,
						Cookie = newCookie,
						Header = newHeader,
						Token = newToken
					}
				});
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
		/// An event that will be raised after jwt work is done, but before next is called
		/// </summary>
		protected AsyncEvent<JwtServiceMiddleware<T>, JwtEventArgs<T>> AfterJwtWorkBeforeNext;

		/// <summary>
		/// An event that will be raised after next is calle, but before jwt work is done
		/// </summary>
		protected AsyncEvent<JwtServiceMiddleware<T>, AfterWorkJwtEventArgs<T>> AfterNextBeforeJwtWork;
	}
}