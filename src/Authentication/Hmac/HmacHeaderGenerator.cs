using System;
using System.Net.Http;

namespace StandardDot.Authentication.Hmac
{
    /// <summary>
    /// The base for an HMAC authentication generator. The scheme, app id, and secret key can be passed in or overridden.
    /// </summary>
    public class HmacHeaderGenerator
    {
        /// <param name="signatureGenerator">The signature generator that will create the header</param>
        /// <param name="customHeaderName">The header name to use for the header</param>
        public HmacHeaderGenerator(HmacSignatureGenerator signatureGenerator = null, string customHeaderName = null)
        {
            SignatureGenerator = signatureGenerator ?? new HmacSignatureGenerator(CustomHeaderScheme);
            HeaderName = customHeaderName ?? "Authorization";
        }

        /// <param name="customHeaderScheme">The header scheme that will be used for generating headers</param>
        /// <param name="signatureGenerator">The signature generator that will create the header</param>
        /// <param name="customHeaderName">The header name to use for the header</param>
        public HmacHeaderGenerator(string customHeaderScheme, HmacSignatureGenerator signatureGenerator = null, string customHeaderName = null)
            : this(signatureGenerator)
        {
            CustomHeaderScheme = customHeaderScheme;
        }

        /// <param name="customHeaderScheme">The header scheme that will be used for generating headers</param>
        /// <param name="appId">The appid that will be used to generate headers</param>
        /// <param name="secretKey">The secret key that will be used to generate headers</param>
        /// <param name="signatureGenerator">The signature generator that will create the header</param>
        /// <param name="customHeaderName">The header name to use for the header</param>
        public HmacHeaderGenerator(string customHeaderScheme, string appId, string secretKey, HmacSignatureGenerator signatureGenerator = null, string customHeaderName = null)
            : this(customHeaderScheme, signatureGenerator)
        {
            AppId = appId;
            SecretKey = secretKey;
        }

        /// <param name="customHeaderScheme">The header scheme that will be used for generating headers</param>
        /// <param name="appId">The appid that will be used to generate headers</param>
        /// <param name="secretKey">The secret key that will be used to generate headers</param>
        /// <param name="signatureGenerator">The signature generator that will create the header</param>
        /// <param name="customHeaderName">The header name to use for the header</param>
        public HmacHeaderGenerator(string customHeaderScheme, string appId, string secretKey, string customHeaderName, HmacSignatureGenerator signatureGenerator = null)
            : this(customHeaderScheme, appId, secretKey, signatureGenerator, customHeaderName)
        { }

        // Obtained from Provider, SecretKey MUST be stored securely
        protected virtual string AppId { get; }

        // Override this with a secure way to get the secret key
        protected virtual string SecretKey { get; }

        // Override this with the scheme being used
        protected virtual string CustomHeaderScheme { get; }

        // Override this if you want to use a different Signature Generator        
        protected virtual HmacSignatureGenerator SignatureGenerator { get; }

        // Override this if you want a different Header Name
        protected virtual string HeaderName { get; }
        
        /// <summary>
        /// Adds the HMAC Authentication headers to the client
        /// </summary>
        /// <param name="client">The client that will be used to make the request</param>
        /// <param name="requestUri">The Uri that the request will be sent to</param>
        /// <param name="method">The method that the request will use</param>
        /// <param name="content">The content of the request</param>
        public virtual void AddHmacHeaders(HttpClient client, Uri requestUri, HttpMethod method, string content = null)
        {
            string requestHttpMethod = method.Method;

            string fullHeader = SignatureGenerator.GenerateFullHmacSignature(requestUri.AbsoluteUri, requestHttpMethod, AppId, SecretKey, content);

            client.DefaultRequestHeaders.Add(HeaderName, fullHeader);
        }
    }
}