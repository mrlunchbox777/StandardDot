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
	public class HmacSignatureGenerator
	{
		/// <param name="customHeaderScheme">The header scheme that will be used for generating headers</param>
		public HmacSignatureGenerator(string customHeaderScheme)
		{
			CustomHeaderScheme = customHeaderScheme;
		}

		// Override this with the scheme being used
		protected virtual string CustomHeaderScheme { get; }

		/// <summary>
		/// Adds the HMAC Authentication headers to the client
		/// </summary>
		/// <param name="requestedResource">The string that represents the resource the request will be sent to</param>
		/// <param name="method">The method that the request will use</param>
		/// <param name="appId">The appid that will be used to generate headers</param>
		/// <param name="secretKey">The secret key that will be used to generate headers</param>
		/// <param name="content">The content of the request, default null</param>
		public virtual string GenerateFullHmacSignature(string requestedResource, string method, string appId, string secretKey,
			string content = null, string nonce = null, DateTime? requestTime = null)
		{
			string requestContentBase64String = string.Empty;

			string requestUriString =
				System.Web.HttpUtility.UrlEncode(requestedResource.ToLower());

			// Calculate UNIX time
			string requestTimeStamp = (requestTime ?? DateTime.UtcNow).UnixTimeStamp().ToString();

			// Create random nonce for each request
			string nonceToUse = nonce ?? Guid.NewGuid().ToString("N");

			// Checking if the request contains body, usually will be null with HTTP GET and DELETE
			if (content != null)
			{
				MD5 md5 = MD5.Create();
				// Hashing the request body, any change in request body will result in different hash, we'll incure message integrity
				byte[] requestContentHash;
				using (Stream contentStream = content.ToStream())
				{
					requestContentHash = md5.ComputeHash(contentStream);
				}
				requestContentBase64String = Convert.ToBase64String(requestContentHash);
			}

			// Creating the raw signature string
			string signatureRawData =
				$"{appId}{method}{requestUriString}{requestTimeStamp}{nonceToUse}{requestContentBase64String}";

			byte[] secretKeyByteArray = Convert.FromBase64String(secretKey);

			byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);

			using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
			{
				byte[] signatureBytes = hmac.ComputeHash(signature);
				string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
				// Setting the values in the Authorization header using custom scheme
				return CustomHeaderScheme + " " +
					$"{appId}:{requestSignatureBase64String}:{nonceToUse}:{requestTimeStamp}";
			}
		}
	}
}