using StandardDot.Abstract;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.Authentication.Hmac;

namespace StandardDot.Authentication.UnitTests.AuthenticationServiceTestObjects
{
	public class IsReplayRequestOverride : AuthenticationService
	{
		public IsReplayRequestOverride(ILoggingService loggingService, IApiKeyService apiKeyService, ICachingService cachingService)
			: base(loggingService, apiKeyService, cachingService)
		{
		}

		public bool CheckReplayRequest(string nonce, string requestTimeStamp)
		{
			return IsReplayRequest(nonce, requestTimeStamp);
		}
	}
}