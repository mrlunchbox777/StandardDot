using System;
using System.Linq;
using System.Net.Http;
using Moq;
using StandardDot.Authentication.Hmac;
using Xunit;

namespace StandardDot.Authentication.UnitTests
{
    public class HmacHeaderGeneratorTests
    {
        [Fact]
        public void AddHmacHeadersTest()
        {
            const string requestedResource = "http://0.0.0.0/stuff?param=4&fun=2";
            HttpMethod method = HttpMethod.Post;
            const string appId = "dfjksdfsdfjlsdfkjldfsej";
            const string secretKey = "GVsVLyUq3U2+7bOdkdCTBemtSM8So98G+5EzunOJEcw=";
            const string content = "a blob of text that constitues content";
            const string customNameSpace = "sds";
            const string signatureValue = "a signature";
            const string customHeaderName = "Authorization";
            Mock<HmacSignatureGenerator> signatureGenerator = new Mock<HmacSignatureGenerator>(MockBehavior.Strict, customNameSpace);
            signatureGenerator.SetupAllProperties();
            signatureGenerator.Setup(x => x.GenerateFullHmacSignature(requestedResource, method.ToString(), appId,
                secretKey, content, It.IsAny<string>(), It.IsAny<DateTime?>())).Returns(signatureValue);
            HmacHeaderGenerator generator = new HmacHeaderGenerator(customNameSpace, appId, secretKey, signatureGenerator.Object, customHeaderName);
            using(HttpClient client = new HttpClient())
            {
                generator.AddHmacHeaders(client, new Uri(requestedResource), HttpMethod.Post, content);
                Assert.Equal(signatureValue, client.DefaultRequestHeaders.GetValues(customHeaderName).Single());
            }
        }
    }
}