using StandardDot.Abstract;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.Authentication.Hmac;

namespace StandardDot.Authentication.UnitTests.AuthenticationServiceTestObjects
{
	public class GetHeaderValuesOverride : AuthenticationService
	{
		public GetHeaderValuesOverride(ILoggingService loggingService, IApiKeyService apiKeyService, ICachingService cachingService)
			: base(loggingService, apiKeyService, cachingService)
		{
		}

		public string[] GetHeaderValues(string header)
		{
			return GetAuthorizationHeaderValues(header);
		}
	}
}