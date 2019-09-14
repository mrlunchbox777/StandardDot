using Microsoft.AspNetCore.Http;

namespace StandardDot.Authentication.Jwt.EventArgs
{
	// Event Args For a JWT
	public class JwtEventArgs<T> : System.EventArgs
	{
		/// <param name="Context">The context for the current request</param>
		public HttpContext Context { get; set; }

		/// <param name="Cookie">The string for the cookie pulled from the request, possibly null</param>
		public string Cookie { get; set; }

		/// <param name="Header">The string for the header pulled from the request, possibly null</param>
		public string Header { get; set; }

		/// <param name="Token">The token pulled from the request, possibly <c>default(T)</c></param>
		public T Token { get; set; }
	}
}