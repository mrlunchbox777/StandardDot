using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using StandardDot.Abstract;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.Abstract.CoreServices.Serialization;
using StandardDot.Authentication.Hmac;
using StandardDot.Authentication.UnitTests.AuthenticationServiceTestObjects;
using StandardDot.Caching;
using StandardDot.CoreExtensions;
using StandardDot.CoreServices.Logging;
using StandardDot.Enums;
using Xunit;

namespace StandardDot.Authentication.UnitTests
{
    public class AuthenticationServiceTests
    {
        [Fact]
        public void GetAuthorizationHeaderValuesTest()
        {
            GetHeaderValuesOverride service = GetService((l, a, c) => new GetHeaderValuesOverride(l, a, c));
            string headerValue = "1:2:3:4";
            string badHeaderValue = "1:2";

            string[] headerValues = service.GetHeaderValues(headerValue);
            Assert.Equal(4, headerValues.Length);
            Assert.Equal("1", headerValues[0]);
            Assert.Equal("2", headerValues[1]);
            Assert.Equal("3", headerValues[2]);
            Assert.Equal("4", headerValues[3]);
            Assert.Null(service.GetHeaderValues(badHeaderValue));
        }

        [Fact]
        public void IsValidRequestTest()
        {
            MemoryCachingService cachingService = new MemoryCachingService(TimeSpan.FromMinutes(5));
            IsValidRequestOverride service = GetService((l, a, c) => new IsValidRequestOverride(l, a, c), cachingService);
            const string badAppId = "badappId";
            const string resource = "/test";
            const string method = "GET";
            const string content = "some content";

            Tuple<bool, HmacIsValidRequestResult> result = service.CheckValidRequest(null, null, null, null, null, null, null);
            Assert.False(result.Item1);
            Assert.Equal(HmacIsValidRequestResult.NoValidResouce, result.Item2);

            result = service.CheckValidRequest(null, resource, null, badAppId, null, null, null);
            Assert.False(result.Item1);
            Assert.Equal(HmacIsValidRequestResult.UnableToFindAppId, result.Item2);

            result = service.CheckValidRequest(null, resource, null, _appId, null, null, null);
            Assert.False(result.Item1);
            Assert.Equal(HmacIsValidRequestResult.ReplayRequest, result.Item2);

            result = service.CheckValidRequest(null, resource, null, _appId, null, "a nonce", null);
            Assert.False(result.Item1);
            Assert.Equal(HmacIsValidRequestResult.ReplayRequest, result.Item2);
            
            ulong badCurrentTime = DateTime.UtcNow.AddMinutes(-30).UnixTimeStamp();
            result = service.CheckValidRequest(null, resource, null, _appId, null, "a nonce", badCurrentTime.ToString());
            Assert.False(result.Item1);
            Assert.Equal(HmacIsValidRequestResult.ReplayRequest, result.Item2);

            ulong goodCurrentTime = DateTime.UtcNow.UnixTimeStamp();
            cachingService.Cache("a nonce", "a nonce");
            result = service.CheckValidRequest(null, resource, null, _appId, null, "a nonce", goodCurrentTime.ToString());
            Assert.False(result.Item1);
            Assert.Equal(HmacIsValidRequestResult.ReplayRequest, result.Item2);

            HmacSignatureGenerator signatureGenerator = new HmacSignatureGenerator(CustomHeaderScheme);
            string fullSignature = signatureGenerator.GenerateFullHmacSignature(resource, method, _appId, _secretKey, content);
            string[] signatureParts = service.GetHeaderValues(fullSignature.Split(" ")[1]);
            result = service.CheckValidRequest(content.ToStream(), resource, method, signatureParts[0], signatureParts[1], signatureParts[2], signatureParts[3]);
            Assert.True(result.Item1);
            Assert.Equal(HmacIsValidRequestResult.NoError, result.Item2);

            fullSignature = signatureGenerator.GenerateFullHmacSignature(resource, method, _appId, _secretKey, content);
            signatureParts = service.GetHeaderValues(fullSignature.Split(" ")[1]);
            result = service.CheckValidRequest(content.ToStream(), resource, method, signatureParts[0], _secretKey, signatureParts[2], signatureParts[3]);
            Assert.False(result.Item1);
            Assert.Equal(HmacIsValidRequestResult.SignaturesMismatch, result.Item2);
        }

        [Fact]
        public void IsReplayRequestTest()
        {
            MemoryCachingService cachingService = new MemoryCachingService(TimeSpan.FromMinutes(5));
            IsReplayRequestOverride service = GetService((l, a, c) => new IsReplayRequestOverride(l, a, c), cachingService);

            const string Key = "alreadyAdded";
            const string GoodKey = "notAdded";
            const string Value = "its here";

            cachingService.Cache(Key, Value);
            Assert.True(service.CheckReplayRequest(Key, null));

            ulong badCurrentTime = DateTime.UtcNow.AddMinutes(-30).UnixTimeStamp();
            Assert.True(service.CheckReplayRequest(GoodKey, badCurrentTime.ToString()));

            ulong goodCurrentTime = DateTime.UtcNow.UnixTimeStamp();
            Assert.False(service.CheckReplayRequest(GoodKey, goodCurrentTime.ToString()));
            Assert.Equal(2, cachingService.Count);
            Assert.Contains(GoodKey, cachingService.Keys);
            Assert.Equal(goodCurrentTime.FromUnixTimeStamp(), cachingService[GoodKey].CachedTime);
            Assert.Equal(GoodKey, cachingService[GoodKey].Value);
        }

        [Fact]
        public void ComputeHashTest()
        {
            ComputeHashOverride service = GetService((l, a, c) => new ComputeHashOverride(l, a, c));

            Stream emptyStream = new MemoryStream();
            Tuple<Stream, byte[], string> emptyResult = service.GetComputeHash(emptyStream);
            Assert.Equal(0, emptyResult.Item1.Length);
            Assert.Null(emptyResult.Item2);
            Assert.Empty(emptyResult.Item3);

            const string contentString = "stuff";
            Stream content = contentString.ToStream();
            Tuple<Stream, byte[], string> result = service.GetComputeHash(content);
            byte[] hashResult;
            using (MD5 hasher = MD5.Create())
            {
                hashResult = hasher.ComputeHash(contentString.GetBytes());
            }
            Assert.Equal(contentString, result.Item1.GetString());
            Assert.Equal(hashResult, result.Item2);
            Assert.Equal(contentString, result.Item3);
        }

        [Fact]
        public void DoAuthorizationTest()
        {
            MemoryCachingService cachingService = new MemoryCachingService(TimeSpan.FromMinutes(5));
            AuthenticationService service = GetService((l, a, c) => new AuthenticationService(l, a, c), cachingService);
            const string badAppId = "badappId";
            const string resource = "/test";
            const string method = "GET";
            const string content = "some content";
            ulong badCurrentTime = DateTime.UtcNow.AddMinutes(-30).UnixTimeStamp();
            ulong goodCurrentTime = DateTime.UtcNow.UnixTimeStamp();
            cachingService.Cache("a nonce", "a nonce");
            HmacSignatureGenerator signatureGenerator = new HmacSignatureGenerator(CustomHeaderScheme);
            string fullSignature = signatureGenerator.GenerateFullHmacSignature(resource, method, _appId, _secretKey, content);

            string hmacAuthenticationValue = CustomHeaderScheme + " " + null;
            Tuple<bool, IEnumerable<HmacIsValidRequestResult>, GenericPrincipal> result = service.DoAuthorization(null, null, null, null, true);
            Assert.Null(result);

            // TODO: More stuff
        }
        
        private static T GetService<T>(Func<ILoggingService, IApiKeyService, ICachingService, T> constructor, MemoryCachingService cachingService = null)
            where T: AuthenticationService
        {
            MemoryCachingService memoryCachingService = cachingService ?? new MemoryCachingService(TimeSpan.FromMinutes(5));
            Json serializationService = new Json();
            CacheLoggingService loggingService = new CacheLoggingService(memoryCachingService, serializationService);
            BasicApiKeyService apiKeyService = new BasicApiKeyService(GetBackingKeys());
            T service = constructor(loggingService, apiKeyService, memoryCachingService);
            return service;
        }

        private static Dictionary<string, string> GetBackingKeys()
        {
            Dictionary<string, string> backingKeys = new Dictionary<string, string>();
            backingKeys.Add(_appId, _secretKey);
            return backingKeys;
        }
        
        private const string _appId = "app1";
        
        private const string _secretKey = "GVsVLyUq3U2+7bOdkdCTBemtSM8So98G+5EzunOJEcw=";

        private const string CustomHeaderScheme = "sds";
    }
}
