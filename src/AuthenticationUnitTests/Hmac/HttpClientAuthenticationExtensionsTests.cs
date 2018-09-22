using System;
using System.Linq;
using System.Net.Http;
using Moq;
using StandardDot.Authentication.Hmac;
using Xunit;

namespace StandardDot.Authentication.UnitTests
{
	public class HttpClientAuthenticationExtensionsTests
	{
		[Fact]
		public void IsHeaderGeneratorRegistered()
		{
			Authentication.Hmac.HttpClientAuthenticationExtensions.RegisterHeaderGenerator(null);
			Mock<HmacSignatureGenerator> signatureGeneratorProxy = new Mock<HmacSignatureGenerator>(MockBehavior.Strict, customNameSpace);
			Mock<HmacHeaderGenerator> headerGeneratorProxy = new Mock<HmacHeaderGenerator>(MockBehavior.Strict, signatureGeneratorProxy.Object, customNameSpace);

			using (HttpClient client = new HttpClient())
			{
				Assert.False(client.IsAnyHeaderGeneratorRegistered());
				Assert.False(headerGeneratorProxy.Object.IsAnyHeaderGeneratorRegistered());
				headerGeneratorProxy.Object.RegisterHeaderGenerator();
				Assert.True(client.IsAnyHeaderGeneratorRegistered());
				Assert.True(headerGeneratorProxy.Object.IsAnyHeaderGeneratorRegistered());
			}
			Authentication.Hmac.HttpClientAuthenticationExtensions.RegisterHeaderGenerator(null);
		}

		[Fact]
		public void AddHmacHeadersTest()
		{
			Authentication.Hmac.HttpClientAuthenticationExtensions.RegisterHeaderGenerator(null);
			Mock<HmacSignatureGenerator> signatureGeneratorProxy = new Mock<HmacSignatureGenerator>(MockBehavior.Strict, customNameSpace);
			Mock<HmacHeaderGenerator> headerGeneratorProxy = new Mock<HmacHeaderGenerator>(MockBehavior.Strict, signatureGeneratorProxy.Object, customNameSpace);

			using (HttpClient client = new HttpClient())
			{
				Uri resource = new Uri(requestedResource);
				Assert.Throws<InvalidOperationException>(() => client.AddHmacHeaders(resource, method));
				headerGeneratorProxy.Setup(x => x.AddHmacHeaders(client, resource, method, null));
				headerGeneratorProxy.Object.RegisterHeaderGenerator();
				client.AddHmacHeaders(resource, method);
				headerGeneratorProxy.Verify(x => x.AddHmacHeaders(client, resource, method, null));
			}
			Authentication.Hmac.HttpClientAuthenticationExtensions.RegisterHeaderGenerator(null);
		}

		[Fact]
		public void AddHmacHeadersContentTest()
		{
			Authentication.Hmac.HttpClientAuthenticationExtensions.RegisterHeaderGenerator(null);
			Mock<HmacSignatureGenerator> signatureGeneratorProxy = new Mock<HmacSignatureGenerator>(MockBehavior.Strict, customNameSpace);
			Mock<HmacHeaderGenerator> headerGeneratorProxy = new Mock<HmacHeaderGenerator>(MockBehavior.Strict, signatureGeneratorProxy.Object, customNameSpace);

			using (HttpClient client = new HttpClient())
			{
				Uri resource = new Uri(requestedResource);
				Assert.Throws<InvalidOperationException>(() => client.AddHmacHeaders(resource, method, content));
				headerGeneratorProxy.Setup(x => x.AddHmacHeaders(client, resource, method, content));
				headerGeneratorProxy.Object.RegisterHeaderGenerator();
				client.AddHmacHeaders(resource, method, content);
				headerGeneratorProxy.Verify(x => x.AddHmacHeaders(client, resource, method, content));
			}
			Authentication.Hmac.HttpClientAuthenticationExtensions.RegisterHeaderGenerator(null);
		}

		[Fact]
		public void AddHmacHeadersWithGeneratorTest()
		{
			Authentication.Hmac.HttpClientAuthenticationExtensions.RegisterHeaderGenerator(null);
			Mock<HmacSignatureGenerator> signatureGeneratorProxy = new Mock<HmacSignatureGenerator>(MockBehavior.Strict, customNameSpace);
			Mock<HmacHeaderGenerator> headerGeneratorProxy = new Mock<HmacHeaderGenerator>(MockBehavior.Strict, signatureGeneratorProxy.Object, customNameSpace);

			using (HttpClient client = new HttpClient())
			{
				Uri resource = new Uri(requestedResource);
				Assert.Throws<NullReferenceException>(() => client.AddHmacHeadersWithGenerator(resource, method, null));
				headerGeneratorProxy.Setup(x => x.AddHmacHeaders(client, resource, method, null));
				client.AddHmacHeadersWithGenerator(resource, method, headerGeneratorProxy.Object);
				headerGeneratorProxy.Verify(x => x.AddHmacHeaders(client, resource, method, null));
			}
			Authentication.Hmac.HttpClientAuthenticationExtensions.RegisterHeaderGenerator(null);
		}

		[Fact]
		public void AddHmacHeadersWithGeneratorWithContentTest()
		{
			Authentication.Hmac.HttpClientAuthenticationExtensions.RegisterHeaderGenerator(null);
			Mock<HmacSignatureGenerator> signatureGeneratorProxy = new Mock<HmacSignatureGenerator>(MockBehavior.Strict, customNameSpace);
			Mock<HmacHeaderGenerator> headerGeneratorProxy = new Mock<HmacHeaderGenerator>(MockBehavior.Strict, signatureGeneratorProxy.Object, customNameSpace);

			using (HttpClient client = new HttpClient())
			{
				Uri resource = new Uri(requestedResource);
				Assert.Throws<NullReferenceException>(() => client.AddHmacHeadersWithGenerator(resource, method, content, null));
				headerGeneratorProxy.Setup(x => x.AddHmacHeaders(client, resource, method, content));
				client.AddHmacHeadersWithGenerator(resource, method, content, headerGeneratorProxy.Object);
				headerGeneratorProxy.Verify(x => x.AddHmacHeaders(client, resource, method, content));
			}
			Authentication.Hmac.HttpClientAuthenticationExtensions.RegisterHeaderGenerator(null);
		}

		private const string requestedResource = "http://0.0.0.0/stuff?param=4&fun=2";

		private HttpMethod method = HttpMethod.Post;

		private const string content = "a blob of text that constitues content";

		private const string customNameSpace = "sds";
	}
}