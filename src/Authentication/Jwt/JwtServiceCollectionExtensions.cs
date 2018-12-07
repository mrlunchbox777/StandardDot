using Microsoft.Extensions.DependencyInjection;

namespace StandardDot.Authentication.Jwt
{
	public static class JwtServiceCollectionExtensions
	{
		public static IServiceCollection AddJwtService(this IServiceCollection serviceCollection)
		{
			JwtServiceArgs args = new JwtServiceArgs();

			return serviceCollection.AddJwtService(args);
		}

		public static IServiceCollection AddJwtService(this IServiceCollection serviceCollection, JwtServiceArgs args)
		{
			JwtService service = new JwtService(args);
			serviceCollection.AddSingleton(service);

			return serviceCollection;
		}
	}
}