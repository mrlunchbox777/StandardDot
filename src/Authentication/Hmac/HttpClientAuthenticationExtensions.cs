using System;
using System.Net;
using System.Net.Http;
using System.Reflection;

/// TODO: Add comments
namespace StandardDot.Authentication.Hmac
{
    public static class HttpClientAuthenticationExtensions
    {
        public static void AddHmacHeaders(this HttpClient client, Uri requestUri, HttpMethod method, string content,
            HmacHeaderGeneratorBase headerGenerator)
        {
            headerGenerator.AddHmacHeaders(client, requestUri, method, content);
        }

        public static void AddHmacHeaders(this HttpClient client, Uri requestUri, HttpMethod method,
            HmacHeaderGeneratorBase headerGenerator)
        {
            client.AddHmacHeaders(requestUri, method, null, headerGenerator);
        }

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