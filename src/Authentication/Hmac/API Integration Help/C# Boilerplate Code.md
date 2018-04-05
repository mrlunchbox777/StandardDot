# C# Boilerplate Code

[Hmac Authentication](/src/Authentication/Hmac/API%20Integration%Help/HMAC%20Authentication%20Help.md)

[Home](/README.md)

```csharp
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

/// TODO: Add comments
namespace Your.App.Namespace
{
    public static class WebClientExtensions
    {
        public static void AddHmacHeaders(this WebClient client, Uri requestUri, HttpMethod method, string content,
            BaseHmacHeaderGenerator headerGenerator = null)
        {
            (headerGenerator ?? new DefaultHmacHeaderGenerator()).AddHmacHeaders(client, requestUri, method, content);
        }

        public static void AddHmacHeaders(this WebClient client, Uri requestUri, HttpMethod method,
            BaseHmacHeaderGenerator headerGenerator = null)
        {
            client.AddHmacHeaders(requestUri, method, null, headerGenerator);
        }

        public static Tuple<HttpStatusCode, string> GetStatusCodeAndDescription(this WebClient client)
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

    public abstract class BaseHmacHeaderGenerator
    {
        // Obtained from Provider, SecretKey MUST be stored securely
        protected abstract string AppId { get; } //= ConfigurationManager.AppSettings["HmacAppId"];
        protected abstract string SecretKey { get; } // override this with a secure way to get the secret key

        public void AddHmacHeaders(WebClient client, Uri requestUri, HttpMethod method, string content = null)
        {
            string requestContentBase64String = string.Empty;

            string requestUriString = System.Web.HttpUtility.UrlEncode(requestUri.AbsoluteUri.ToLower());

            string requestHttpMethod = method.Method;

            // Calculate UNIX time
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = DateTime.UtcNow - epochStart;
            string requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();

            // Create random nonce for each request
            string nonce = Guid.NewGuid().ToString("N");

            // Checking if the request contains body, usually will be null with HTTP GET and DELETE
            if (content != null)
            {
                MD5 md5 = MD5.Create();
                // Hashing the request body, any change in request body will result in different hash, we'll incure message integrity
                byte[] requestContentHash;
                using (Stream contentStream = content.GetStreamFromString())
                {
                    requestContentHash = md5.ComputeHash(contentStream);
                }
                requestContentBase64String = Convert.ToBase64String(requestContentHash);
            }

            // Creating the raw signature string
            string signatureRawData =
                $"{AppId}{requestHttpMethod}{requestUriString}{requestTimeStamp}{nonce}{requestContentBase64String}";

            byte[] secretKeyByteArray = Convert.FromBase64String(SecretKey);

            byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);

            using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
                // Setting the values in the Authorization header using custom scheme (sds)
                client.Headers.Add("Authorization","sds " +
                    $"{AppId}:{requestSignatureBase64String}:{nonce}:{requestTimeStamp}");
            }
        }
    }

    public class DefaultHmacHeaderGenerator : BaseHmacHeaderGenerator
    {
        protected override string AppId { get; } = ConfigurationManager.AppSettings["HmacAppId"];
        protected override string SecretKey { get; } = null; // override this with a secure way to get the secret key
    }
}
```