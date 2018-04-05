using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using StandardDot.Abstract;
using StandardDot.Abstract.Caching;
using StandardDot.CoreExtensions;
using StandardDot.Enums;

namespace StandardDot.Hmac
{
    /// <summary>
    /// Handles HMAC Authentication, please read the documentation under API Integration Help
    /// </summary>
    public class AuthenticationService
    {
        /// <summary>
        /// Create an Authentication Service that only requires other services
        /// </summary>
        /// <param name="loggingService">The service to write logs to</param>
        /// <param name="apiKeyService">The service that provides Api Keys</param>
        /// <param name="cachingService">The service that handles caching</param>
        public AuthenticationService(ILoggingService loggingService, IApiKeyService apiKeyService, ICachingService cachingService)
            : this(loggingService, apiKeyService, cachingService, 300, 300, "sds")
        {
        }

        /// <summary>
        /// Create an Authentication Service that only requires other services
        /// </summary>
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
        private string[] GetAutherizationHeaderValues(string hmacAuthenticationValue)
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
        /// <returns>(Is the request valid, The error message)</returns>
        private Tuple<bool, string> IsValidRequest(Stream content, string resource, string resourceRequestMethod, string appId,
            string incomingBase64Signature, string nonce, string requestTimeStamp)
        {
            if (string.IsNullOrWhiteSpace(resource))
            {
                LoggingService.LogMessage("HMAC Auth Resource Not Valid",
                    "No valid request requested resource", LogLevel.Debug);
                return new Tuple<bool, string>(false, "No Valid Resource");
            }

            string requestContentBase64String = "";

            string sharedKey = ApiKeyService[appId];
            if (string.IsNullOrWhiteSpace(sharedKey))
            {
                LoggingService.LogMessage("Api Auth Request Not Valid",
                    "Unable to find appId", LogLevel.Debug);
                return new Tuple<bool, string>(false, "Unable to find appId. Is your app active?");
            }

            if (IsReplayRequest(nonce, requestTimeStamp))
            {
                LoggingService.LogMessage("Api Auth Request Not Valid",
                    "Looked like a replay request.\r\nNonce - " + nonce + "\r\nRequest Timestamp - " + requestTimeStamp
                    + "\r\nOur Timestamp" + (DateTime.UtcNow - Constants.DateTime.UnixEpoch).TotalSeconds, LogLevel.Debug);
                return new Tuple<bool, string>(false,
                    "This looks like a replay request. Are you creating a new nonce every time?");
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
                return new Tuple<bool, string>(matched, matched ? "" : "Signatures didn't match.");
            }

        }

        /// <summary>
        /// Ensures that the request recognizes the correct time, and that it is not a repeat request (based on nonce recognition)
        /// </summary>
        /// <param name="contentStream">The stream of content to hash</param>
        /// <returns>If the request is a replay request</returns>
        private bool IsReplayRequest(string nonce, string requestTimeStamp)
        {
            if (CachingService.ContainsKey(nonce))
            {
                return true;
            }

            TimeSpan currentTs = DateTime.UtcNow - Constants.DateTime.UnixEpoch;

            ulong serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            ulong requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);

            if ((serverTotalSeconds - requestTotalSeconds) > ServerClientMaxTimeDifference)
            {
                return true;
            }

            CachingService.Cache(nonce, nonce, requestTimeStamp?.GetDateTimeFromUnixTimestampString() ?? DateTime.MinValue,
                DateTime.UtcNow.AddSeconds(RequestMaxAgeInSeconds));

            return false;
        }

        /// <summary>
        /// Computes the hash of the content, and returns the content in several different ways
        /// </summary>
        /// <param name="contentStream">The stream of content to hash</param>
        /// <returns>(A copy of the contentStream, the hash of the content, the string representing the content)</returns>
        private static Tuple<Stream, byte[], string> ComputeHash(Stream contentStream)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] content = contentStream.GetByteArrayFromStream();

                string rawContent = "";
                if (content.Length == 0)
                {
                    return new Tuple<Stream, byte[], string>(new MemoryStream(), null, rawContent);
                }

                byte[] hash = md5.ComputeHash(content);
                rawContent = content.GetString();
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
        public Tuple<bool, string, GenericPrincipal> DoAuthorization(string hmacAuthenticationValue,
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

                string[] autherizationHeaderArray = GetAutherizationHeaderValues(rawAuthValue);

                if (autherizationHeaderArray != null && autherizationHeaderArray.Length == 4)
                {
                    string appId = autherizationHeaderArray[0];
                    string incomingBase64Signature = autherizationHeaderArray[1];
                    string nonce = autherizationHeaderArray[2];
                    string requestTimeStamp = autherizationHeaderArray[3];

                    Tuple<bool, string> isValid =
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
                        return new Tuple<bool, string, GenericPrincipal>(false, isValid.Item2, null);
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