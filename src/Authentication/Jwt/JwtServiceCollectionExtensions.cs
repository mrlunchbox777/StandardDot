using Microsoft.Extensions.DependencyInjection;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// The extension methods to add a <see cref="JwtService" /> with
	/// </summary>
	public static class JwtServiceCollectionExtensions
	{
		/// <summary>
		/// Adds a new <see cref="JwtService" /> with defaults as a Singleton to the passed <see cref="IServiceCollection" />
		/// </summary>
		/// <param name="serviceCollection">The current <see cref="IServiceCollection" /></param>
		/// <returns>The current <see cref="IServiceCollection" /></returns>
		public static IServiceCollection AddJwtService(this IServiceCollection serviceCollection)
		{
			JwtServiceArgs args = new JwtServiceArgs();

			return serviceCollection.AddJwtService(args);
		}

		/// <summary>
		/// Adds a new <see cref="JwtService" /> with the passed args as a Singleton to the passed <see cref="IServiceCollection" />
		/// </summary>
		/// <param name="serviceCollection">The current <see cref="IServiceCollection" /></param>
		/// <param name="args">The arguments to create the <see cref="JwtService" /> with</param>
		/// <returns>The current <see cref="IServiceCollection" /></returns>
		public static IServiceCollection AddJwtService(this IServiceCollection serviceCollection, JwtServiceArgs args)
		{
			JwtService service = new JwtService(args);
			serviceCollection.AddSingleton(service);

			return serviceCollection;
		}
	}
}
