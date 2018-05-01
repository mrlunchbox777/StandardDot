using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using StandardDot.CoreExtensions;

namespace StandardDot.Authentication.Hmac
{
    /// <summary>
    /// The base for an HMAC authentication generator. The scheme, app id, and secret key can be passed in or overridden.
    /// </summary>
    public abstract class HmacHeaderGeneratorBase
    {
        public HmacHeaderGeneratorBase()
        {
            SignatureGenerator = new HmacSignatureGenerator(CustomHeaderScheme);
        }

        /// <param name="customHeaderScheme">The header scheme that will be used for generating headers</param>
        public HmacHeaderGeneratorBase(string customHeaderScheme)
        {
            CustomHeaderScheme = customHeaderScheme;
            SignatureGenerator = new HmacSignatureGenerator(CustomHeaderScheme);
        }

        /// <param name="customHeaderScheme">The header scheme that will be used for generating headers</param>
        /// <param name="appId">The appid that will be used to generate headers</param>
        /// <param name="secretKey">The secret key that will be used to generate headers</param>
        public HmacHeaderGeneratorBase(string customHeaderScheme, string appId, string secretKey)
            : this(customHeaderScheme)
        {
            AppId = appId;
            SecretKey = secretKey;
        }

        // Obtained from Provider, SecretKey MUST be stored securely
        protected virtual string AppId { get; }

        // Override this with a secure way to get the secret key
        protected virtual string SecretKey { get; }

        // Override this with the scheme being used
        protected virtual string CustomHeaderScheme { get; }
        
        protected virtual HmacSignatureGenerator SignatureGenerator { get; }

        /// <summary>
        /// Adds the HMAC Authentication headers to the client
        /// </summary>
        /// <param name="client">The client that will be used to make the request</param>
        /// <param name="requestUri">The Uri that the request will be sent to</param>
        /// <param name="method">The method that the request will use</param>
        /// <param name="content">The content of the request</param>
        public virtual void AddHmacHeaders(HttpClient client, Uri requestUri, HttpMethod method, string content = null)
        {
            string requestContentBase64String = string.Empty;

            string requestUriString = System.Web.HttpUtility.UrlEncode(requestUri.AbsoluteUri.ToLower());

            string requestHttpMethod = method.Method;

            string fullHeader = SignatureGenerator.GenerateFullHmacSignature(requestUri.AbsoluteUri, requestHttpMethod, AppId, SecretKey, content);

            client.DefaultRequestHeaders.Add("Authorization", fullHeader);
        }
    }
}