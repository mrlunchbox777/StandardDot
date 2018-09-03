using System;
using System.IO;
using StandardDot.Abstract;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.Authentication.Hmac;

namespace StandardDot.Authentication.IntegrationTests.AuthenticationServiceTestObjects
{
    public class ComputeHashOverride : AuthenticationService
    {
        public ComputeHashOverride(ILoggingService loggingService, IApiKeyService apiKeyService, ICachingService cachingService)
            : base(loggingService, apiKeyService, cachingService)
        {
        }

        public Tuple<Stream, byte[], string> GetComputeHash(Stream content)
        {
            return ComputeHash(content);
        }
    }
}