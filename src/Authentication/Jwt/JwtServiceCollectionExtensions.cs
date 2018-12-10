// using Microsoft.Extensions.DependencyInjection;

// namespace StandardDot.Authentication.Jwt
// {
// 	public static class JwtServiceCollectionExtensions
// 	{
// 		public static IServiceCollection AddJwtService(this IServiceCollection serviceCollection)
// 		{
// 			JwtServiceArgs args = new JwtServiceArgs();

// 			return serviceCollection.AddJwtService(args);
// 		}

// 		public static IServiceCollection AddJwtService(this IServiceCollection serviceCollection, JwtServiceArgs args)
// 		{
// 			JwtService service = new JwtService(args.Secret, args.Serializer, args.Provider, args.Validator, args.UrlEncoder, args.Decoder, args.Algorithm, args.Encoder);
// 			serviceCollection.AddSingleton(service);

// 			return serviceCollection;
// 		}
// 	}
// }