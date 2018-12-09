using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// The middleware for a <see cref="JwtService" />
	/// </summary>
	/// <typeparam name="T">The type for the JWT</typeparam>
	public class JwtServiceMiddleware<T>
		where T : IJwtToken
	{
		/// <param name="next">The next middleware to run</param>
		/// <param name="jwtExpiration">The Expiration for the JWT, default 1 hour</param>
		/// <param name="jwtService">The service to use in the middleware, default <see cref="JwtService" /> with no params</param>
		/// <param name="jwtIdentifier">The key that identifies the JWT, default <c>"StandardDotJwt"</c></param>
		public JwtServiceMiddleware(RequestDelegate next, TimeSpan? jwtExpiration = null, string jwtIdentifier = null, JwtService jwtService = null)
		{
			_next = next;
			_jwtIdentifier = jwtIdentifier ?? "StandardDotJwt";
			_jwtService = jwtService ?? new JwtService();
			_jwtExpiration = jwtExpiration ?? _jwtExpiration;
		}

		private readonly TimeSpan _jwtExpiration = new TimeSpan(1, 0, 0);

		protected readonly RequestDelegate _next;
		
		protected readonly string _jwtIdentifier;
		
		protected readonly JwtService _jwtService;

		/// <summary>
		/// Runs the middleware
		/// </summary>
		/// <param name="context">The context for the current request</param>
		/// <returns>A Task for when this request has completed</returns>
		public virtual async Task InvokeAsync(HttpContext context)
		{
			context.Items.Add(_jwtIdentifier, _jwtService.DecodeJwtPayload<T>(context?.Request?.Cookies?[_jwtIdentifier]));
			// Call the next delegate/middleware in the pipeline
			await _next(context);
			// do after context work such as updating the jwt as shown below
			if (!context.Response.HasStarted)
			{
				IJwtToken token = _jwtService.DecodeJwtPayload<T>(context?.Request?.Cookies?[_jwtIdentifier]);
				token.Expiration = DateTime.UtcNow.Add(_jwtExpiration);
				context.Response.Cookies.Append(_jwtIdentifier, _jwtService.EncodeJwtPayload(token), new CookieOptions {Expires = token.Expiration});
			}
		}
	}
}