using System;
using System.Security.Cryptography;
using System.Text;
using StandardDot.Authentication.Hmac;
using StandardDot.CoreExtensions;
using Xunit;

namespace StandardDot.Authentication.IntegrationTests
{
    public class HmacSignatureGeneratorTests
    {
        [Fact]
        public void ContentHeaderTest()
        {
            string requestedResource = "/stuff?param=4&fun=2";
            string method = "PoSt";
            string appId = "dfjksdfsdfjlsdfkjldfsej";
            string secretKey = "GVsVLyUq3U2+7bOdkdCTBemtSM8So98G+5EzunOJEcw=";
            string content = "a blob of text that constitues content";
            string customNameSpace = "sds";
            string base64Content = "C6Gkryxz0SfwxmZKzjLCnQ==";
            string encodedRequestUri = "%2fstuff%3fparam%3d4%26fun%3d2";
            string nonce = Guid.NewGuid().ToString("N");
            DateTime requestTime = DateTime.UtcNow;

            HmacSignatureGenerator generator = new HmacSignatureGenerator(customNameSpace);

            string fullHeader = GenerateFullHmacSignature(nonce, customNameSpace, requestTime, base64Content, encodedRequestUri, method, appId, secretKey);
            Assert.NotEmpty(fullHeader);
            Assert.Equal(fullHeader, generator.GenerateFullHmacSignature(requestedResource, method, appId, secretKey, content, nonce, requestTime));
        }

        [Fact]
        public void ContentlessHeaderTest()
        {
            string requestedResource = "/stuff?param=4&fun=2";
            string method = "PoSt";
            string appId = "dfjksdfsdfjlsdfkjldfsej";
            string secretKey = "GVsVLyUq3U2+7bOdkdCTBemtSM8So98G+5EzunOJEcw=";
            string customNameSpace = "sds";
            string base64Content = null;
            string encodedRequestUri = "%2fstuff%3fparam%3d4%26fun%3d2";
            string nonce = Guid.NewGuid().ToString("N");
            DateTime requestTime = DateTime.UtcNow;

            HmacSignatureGenerator generator = new HmacSignatureGenerator(customNameSpace);

            string fullHeader = GenerateFullHmacSignature(nonce, customNameSpace, requestTime, base64Content, encodedRequestUri, method, appId, secretKey);
            Assert.NotEmpty(fullHeader);
            Assert.Equal(fullHeader, generator.GenerateFullHmacSignature(requestedResource, method, appId, secretKey, null, nonce, requestTime));
        }

        // Maybe put more branching testing in here, but that is pretty well tested with the Authentication Service Tests
        // It's just missing testing for the default values (generating a nonce and using utc now as the date time)
        
        public virtual string GenerateFullHmacSignature(string nonce, string customNameSpace, DateTime requestTime,
            string requestContentBase64String, string requestUriString, string method, string appId, string secretKey)
        {
            string requestTimeStamp = requestTime.UnixTimeStamp().ToString();

            // Creating the raw signature string
            string signatureRawData =
                $"{appId}{method}{requestUriString}{requestTimeStamp}{nonce}{requestContentBase64String}";

            byte[] secretKeyByteArray = Convert.FromBase64String(secretKey);

            byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);

            using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
                // Setting the values in the Authorization header using custom scheme
                return customNameSpace + " " +
                    $"{appId}:{requestSignatureBase64String}:{nonce}:{requestTimeStamp}";
            }
        }
    }
}