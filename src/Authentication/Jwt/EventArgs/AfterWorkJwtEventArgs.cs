namespace StandardDot.Authentication.Jwt.EventArgs
{
	// Event Args For a JWT after work
	public class AfterWorkJwtEventArgs<T> : System.EventArgs
	{
		/// <param name="BeforeArgs">The args for the before work</param>
		public JwtEventArgs<T> BeforeArgs { get; set; }

		/// <param name="AfterArgs">The args for the after work</param>
		public JwtEventArgs<T> AfterArgs { get; set; }
	}
}