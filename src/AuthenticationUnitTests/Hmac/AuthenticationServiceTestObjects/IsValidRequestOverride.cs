using System;
using System.IO;
using StandardDot.Abstract;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.Authentication.Hmac;
using StandardDot.Enums;

namespace StandardDot.Authentication.UnitTests.AuthenticationServiceTestObjects
{
    public class IsValidRequestOverride : AuthenticationService
    {
        public IsValidRequestOverride(ILoggingService loggingService, IApiKeyService apiKeyService, ICachingService cachingService)
            : base(loggingService, apiKeyService, cachingService)
        {
        }

        public Tuple<bool, HmacIsValidRequestResult> CheckValidRequest(Stream content, string resource, string resourceRequestMethod, string appId,
            string incomingBase64Signature, string nonce, string requestTimeStamp)
        {
            return IsValidRequest(content, resource, resourceRequestMethod, appId,
                incomingBase64Signature, nonce, requestTimeStamp);
        }
    }
}