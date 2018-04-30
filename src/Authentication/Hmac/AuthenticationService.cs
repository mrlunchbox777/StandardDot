using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using StandardDot.Abstract;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.CoreExtensions;
using StandardDot.Enums;

namespace StandardDot.Authentication.Hmac
{
    /// <summary>
    /// Handles HMAC Authentication, please read the documentation under API Integration Help
    /// </summary>
    public class AuthenticationService
    {
        /// <param name="loggingService">The service to write logs to</param>
        /// <param name="apiKeyService">The service that provides Api Keys</param>
        /// <param name="cachingService">The service that handles caching</param>
        public AuthenticationService(ILoggingService loggingService, IApiKeyService apiKeyService, ICachingService cachingService)
            : this(loggingService, apiKeyService, cachingService, 300, 300, "sds")
        { }

        /// <param name="loggingService">The service to write logs to</param>
        /// <param name="apiKeyService">The service that provides Api Keys</param>
        /// <param name="cachingService">The service that handles caching</param>
        /// <param name="requestMaxAgeInSeconds">How long to store a nonce to prevent replay requests, default 300</param>
        /// <param name="serverClientMaxTimeDifference">The number of seconds that the requester is allowed to be off, default 300</param>
        /// <param name="authenticationScheme">The scheme that identifies this authentication, default sds</param>
        public AuthenticationService(ILoggingService loggingService, IApiKeyService apiKeyService, ICachingService cachingService,
            ulong requestMaxAgeInSeconds, ulong serverClientMaxTimeDifference, string authenticationScheme)
        {
            RequestMaxAgeInSeconds = requestMaxAgeInSeconds;
            AuthenticationScheme = authenticationScheme;
            LoggingService = loggingService;
            ApiKeyService = apiKeyService;
            CachingService = cachingService;
            ServerClientMaxTimeDifference = serverClientMaxTimeDifference;
        }

        protected virtual ILoggingService LoggingService { get; }

        protected virtual IApiKeyService ApiKeyService { get; }

        protected virtual ICachingService CachingService { get; }

        protected virtual ulong RequestMaxAgeInSeconds { get; }

        protected virtual ulong ServerClientMaxTimeDifference { get; }

        protected virtual string AuthenticationScheme { get; }

        /// <summary>
        /// Gets the different parts of the HMAC Authentication Value
        /// </summary>
        /// <param name="hmacAuthenticationValue">The authentication value that the client passed</param>
        /// <returns>An array of the parts of the HMAC Authentication Value</returns>
        protected virtual string[] GetAuthorizationHeaderValues(string hmacAuthenticationValue)
        {
            string[] credArray = hmacAuthenticationValue.Split(':');

            if (credArray.Length == 4)
            {
                return credArray;
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Checks if a request is valid based on what the client passed
        /// </summary>
        /// <param name="content">The content that the client passed</param>
        /// <param name="resource">The resource that the client requested (URI ENCODED)</param>
        /// <param name="resourceRequestMethod">The method that the client used to request the resource (ALL UPPER)</param>
        /// <param name="appId">The appId the client is claiming to be</param>
        /// <param name="incomingBase64Signature">The base64 signature the client passed</param>
        /// <param name="nonce">The nonce the client provided to identify the request</param>
        /// <param name="requestTimeStamp">The timestamp the client provided to identify the request</param>
        /// <returns>(Is the request valid, The error)</returns>
        protected virtual Tuple<bool, HmacIsValidRequestResult> IsValidRequest(Stream content, string resource, string resourceRequestMethod, string appId,
            string incomingBase64Signature, string nonce, string requestTimeStamp)
        {
            if (string.IsNullOrWhiteSpace(resource))
            {
                LoggingService.LogMessage("HMAC Auth Resource Not Valid",
                    "No valid request requested resource", LogLevel.Debug);
                return new Tuple<bool, HmacIsValidRequestResult>(false, HmacIsValidRequestResult.NoValidResouce);
            }

            string requestContentBase64String = "";

            ApiKeyService.TryGetValue(appId, out string sharedKey);
            if (string.IsNullOrWhiteSpace(sharedKey))
            {
                LoggingService.LogMessage("Api Auth Request Not Valid",
                    "Unable to find appId", LogLevel.Debug);
                return new Tuple<bool, HmacIsValidRequestResult>(false, HmacIsValidRequestResult.UnableToFindAppId);
            }

            if (IsReplayRequest(nonce, requestTimeStamp))
            {
                LoggingService.LogMessage("Api Auth Request Not Valid",
                    "Looked like a replay request.\r\nNonce - " + nonce + "\r\nRequest Timestamp - " + requestTimeStamp
                    + "\r\nOur Timestamp" + (DateTime.UtcNow - Constants.DateTime.UnixEpoch).TotalSeconds, LogLevel.Debug);
                return new Tuple<bool, HmacIsValidRequestResult>(false,
                    HmacIsValidRequestResult.ReplayRequest);
            }

            // we need a content stream (we need to make sure we pass this back)
            Tuple<Stream, byte[], string> hashAndContent = ComputeHash(content);

            if (hashAndContent.Item2 != null)
            {
                requestContentBase64String = Convert.ToBase64String(hashAndContent.Item2);
            }

            string data =
                $"{appId}{resourceRequestMethod}{resource}{requestTimeStamp}{nonce}{requestContentBase64String}";

            byte[] secretKeyBytes = Convert.FromBase64String(sharedKey);

            byte[] signature = data.GetBytes();

            using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);

                string signatureString = Convert.ToBase64String(signatureBytes);
                bool matched = incomingBase64Signature.Equals(signatureString,
                    StringComparison.Ordinal);
                if (!matched)
                {
                    LoggingService.LogMessage("Api Auth Request Not Valid",
                        "Signatures didn't match.\r\nRequest Signature - " + incomingBase64Signature
                        + ".\r\nInternal Signature - " + signatureString + ".\r\nInternal signature data - " + data
                        + "\r\nRaw Content - " + hashAndContent.Item2,
                        LogLevel.Debug);
                }
                return new Tuple<bool, HmacIsValidRequestResult>(matched, matched
                    ? HmacIsValidRequestResult.NoError
                    : HmacIsValidRequestResult.SignaturesMismatch);
            }

        }

        /// <summary>
        /// Ensures that the request recognizes the correct time, and that it is not a repeat request (based on nonce recognition)
        /// </summary>
        /// <param name="contentStream">The stream of content to hash</param>
        /// <returns>If the request is a replay request</returns>
        protected virtual bool IsReplayRequest(string nonce, string requestTimeStamp)
        {
            if (CachingService.ContainsKey(nonce))
            {
                return true;
            }

            ulong serverTotalSeconds = DateTime.UtcNow.UnixTimeStamp();
            ulong requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);

            if ((serverTotalSeconds - requestTotalSeconds) > ServerClientMaxTimeDifference)
            {
                return true;
            }

            DateTime cachedTime = requestTimeStamp?.GetDateTimeFromUnixTimestampString() ?? DateTime.MinValue;
            CachingService.Cache(nonce, nonce, cachedTime, cachedTime.AddSeconds(RequestMaxAgeInSeconds));

            return false;
        }

        /// <summary>
        /// Computes the hash of the content, and returns the content in several different ways
        /// </summary>
        /// <param name="contentStream">The stream of content to hash</param>
        /// <returns>(A copy of the contentStream, the hash of the content, the string representing the content)</returns>
        protected virtual Tuple<Stream, byte[], string> ComputeHash(Stream contentStream)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] content = contentStream.ToByteArray();

                string rawContent = "";
                if (content.Length == 0)
                {
                    return new Tuple<Stream, byte[], string>(new MemoryStream(), null, rawContent);
                }

                byte[] hash = md5.ComputeHash(content);
                rawContent = content.FromBytes();
                return new Tuple<Stream, byte[], string>(new MemoryStream(content), hash, rawContent);
            }
        }

        /// <summary>
        /// Authorizes a request using HMAC authentication
        /// </summary>
        /// <param name="hmacAuthenticationValue">The authentication value that the client passed (usually from the headers)</param>
        /// <param name="content">The content that the client passed</param>
        /// <param name="resource">The resource that the client requested (URI ENCODED)</param>
        /// <param name="resourceRequestMethod">The method that the client used to request the resource (ALL UPPER)</param>
        /// <param name="allowAnonymous">Should allow anonymous requests</param>
        /// <returns>(Is Authorized, Error Message, A Principal based on the AppId)</returns>
        public virtual Tuple<bool, string, GenericPrincipal> DoAuthorization(string hmacAuthenticationValue,
            Stream content, string resource, string resourceRequestMethod, bool allowAnonymous)
        {
            // If anonymous access is allowed the client should be authorized no matter what
            if (allowAnonymous)
            {
                return null;
            }

            string[] authParts = hmacAuthenticationValue?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (hmacAuthenticationValue != null && authParts.Length > 0
                && AuthenticationScheme.Equals(authParts[0].Trim(), StringComparison.OrdinalIgnoreCase))
            {
                string rawAuthValue = authParts[1];

                string[] autherizationHeaderArray = GetAuthorizationHeaderValues(rawAuthValue);

                if (autherizationHeaderArray != null && autherizationHeaderArray.Length == 4)
                {
                    string appId = autherizationHeaderArray[0];
                    string incomingBase64Signature = autherizationHeaderArray[1];
                    string nonce = autherizationHeaderArray[2];
                    string requestTimeStamp = autherizationHeaderArray[3];

                    Tuple<bool, HmacIsValidRequestResult> isValid =
                        IsValidRequest(content, resource, resourceRequestMethod, appId, incomingBase64Signature, nonce, requestTimeStamp);

                    if (isValid.Item1)
                    {
                        GenericPrincipal currentPrincipal = new GenericPrincipal(new GenericIdentity(appId), null);
                        return new Tuple<bool, string, GenericPrincipal>(true, null, currentPrincipal);
                    }
                    else
                    {
                        LoggingService.LogMessage("Bad Hmac Auth",
                            "\r\nappId - " + appId + "\r\nincomingBase64Signature - " + incomingBase64Signature
                            + "\r\nnonce - " + nonce + "\r\nrequestTimeStamp - " + requestTimeStamp + ". Message - "
                            + isValid.Item2, LogLevel.Info);
                        return new Tuple<bool, string, GenericPrincipal>(false, isValid.Item2.ToString(), null);
                    }
                }
                else
                {
                    string errorMessage = "Incorrect items found in the Hmac Authorization Value. Parts should be 4 found "
                                                + (autherizationHeaderArray?.Length.ToString() ?? "0");
                    LoggingService.LogMessage("Bad Hmac Auth", errorMessage, LogLevel.Info);
                    return new Tuple<bool, string, GenericPrincipal>(false, errorMessage, null);
                }
            }
            else
            {
                string errorMessage = (string.IsNullOrWhiteSpace(hmacAuthenticationValue) ? "No Hmac Authorization Value." : "")
                                    + (authParts?.Length > 0
                                        ? "Not enough auth parts. Did you include the namespace and parameter string?"
                                        : "")
                                    + (AuthenticationScheme.Equals(authParts?[0]?.Trim(), StringComparison.OrdinalIgnoreCase)
                                        ? "Improper namespace. Did you include it?"
                                        : "");
                LoggingService.LogMessage("Bad Api Auth", errorMessage, LogLevel.Info);
                return new Tuple<bool, string, GenericPrincipal>(false, errorMessage, null);
            }
        }
    }
}