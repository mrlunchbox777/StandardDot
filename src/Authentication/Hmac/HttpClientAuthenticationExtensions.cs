using System;
using System.Net;
using System.Net.Http;
using System.Reflection;

/// TODO: Add comments
namespace StandardDot.Authentication.Hmac
{
    /// <summary>
    /// Extensions for the HttpClient to add headers
    /// </summary>
    public static class HttpClientAuthenticationExtensions
    {
        private static HmacHeaderGenerator _registeredGenerator = null;

        public static bool IsAnyHeaderGeneratorRegisteredProp => _registeredGenerator != null;

        /// <summary>
        /// Checks if there is a registered headergenerator
        /// </summary>
        /// <param name="client">The client that will be used to make the request</param>
        /// <returns>If there is a registered header generator</returns>
        public static bool IsAnyHeaderGeneratorRegistered(this HttpClient client)
        {
            return IsAnyHeaderGeneratorRegisteredProp;
        }

        /// <summary>
        /// Checks if there is a registered headergenerator
        /// </summary>
        /// <param name="headerGenerator">The header generator to use (this will not be registered)</param>
        /// <returns>If there is a registered header generator</returns>
        public static bool IsAnyHeaderGeneratorRegistered(this HmacHeaderGenerator headerGenerator)
        {
            return IsAnyHeaderGeneratorRegisteredProp;
        }

        /// <summary>
        /// Registers a header generator, overwrites the currently registered header generator if applicable
        /// </summary>
        /// <param name="headerGenerator">The header generator to register</param>
        public static void RegisterHeaderGenerator(this HmacHeaderGenerator headerGenerator)
        {
            _registeredGenerator = headerGenerator;
        }

        /// <summary>
        /// Adds an HMAC header to a HttpClient using the provided header generator
        /// </summary>
        /// <param name="client">The client that will be used to make the request</param>
        /// <param name="requestUri">The Uri that the request will be sent to</param>
        /// <param name="method">The method that the request will be used</param>
        /// <param name="content">The content of the request</param>
        /// <param name="headerGenerator">The header generator to use (this will not be registered)</param>
        public static void AddHmacHeadersWithGenerator(this HttpClient client, Uri requestUri, HttpMethod method, string content,
            HmacHeaderGenerator headerGenerator)
        {
            if (headerGenerator == null)
            {
                throw new NullReferenceException("No valid header generator passed.");
            }
            headerGenerator.AddHmacHeaders(client, requestUri, method, content);
        }

        /// <summary>
        /// Adds an HMAC header to a HttpClient using the provided header generator
        /// </summary>
        /// <param name="client">The client that will be used to make the request</param>
        /// <param name="requestUri">The Uri that the request will be sent to</param>
        /// <param name="method">The method that the request will be used</param>
        /// <param name="headerGenerator">The header generator to use (this will not be registered)</param>
        public static void AddHmacHeadersWithGenerator(this HttpClient client, Uri requestUri, HttpMethod method,
            HmacHeaderGenerator headerGenerator)
        {
            if (headerGenerator == null)
            {
                throw new NullReferenceException("No valid header generator passed.");
            }
            client.AddHmacHeadersWithGenerator(requestUri, method, null, headerGenerator);
        }

        /// <summary>
        /// Adds an HMAC header to a HttpClient using the registered header generator
        /// </summary>
        /// <param name="client">The client that will be used to make the request</param>
        /// <param name="requestUri">The Uri that the request will be sent to</param>
        /// <param name="method">The method that the request will be used</param>
        /// <param name="content">The content of the request</param>
        public static void AddHmacHeaders(this HttpClient client, Uri requestUri, HttpMethod method, string content)
        {
            if (!IsAnyHeaderGeneratorRegisteredProp)
            {
                throw new InvalidOperationException("Unable to add headers without a registered generator. Please register a generator first.");
            }
            _registeredGenerator.AddHmacHeaders(client, requestUri, method, content);
        }

        /// <summary>
        /// Adds an HMAC header to a HttpClient using the registered header generator
        /// </summary>
        /// <param name="client">The client that will be used to make the request</param>
        /// <param name="requestUri">The Uri that the request will be sent to</param>
        /// <param name="method">The method that the request will be used</param>
        public static void AddHmacHeaders(this HttpClient client, Uri requestUri, HttpMethod method)
        {
            client.AddHmacHeaders(requestUri, method, null);
        }

        /// <summary>
        /// Adds an HMAC header to a HttpClient using the registered header generator
        /// </summary>
        /// <param name="client">The client that will be used to make the request</param>
        /// <returns>(The http code returned in the response, the http code description returned in the response)</returns>
        public static Tuple<HttpStatusCode, string> GetStatusCodeAndDescription(this HttpClient client)
        {
            FieldInfo responseField = client.GetType().GetField("m_WebResponse", BindingFlags.Instance | BindingFlags.NonPublic);
            if (responseField == null)
            {
                return null;
            }

            HttpWebResponse response = responseField.GetValue(client) as HttpWebResponse;
            if (response == null)
            {
                return null;
            }

            string statusDescription = response.StatusDescription;
            HttpStatusCode code = response.StatusCode;
            return new Tuple<HttpStatusCode, string>(code, statusDescription);
        }
    }
}