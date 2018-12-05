using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace StandardDot.CoreExtensions.UnitTests
{
	public class SecureStringExtensionsTests
	{
		[Fact]
		public void ToPlainTextTest()
		{
			string val = "this is a test string";
			SecureString secure = val.ToSecureString();
			string decrypted;
			IntPtr valuePtr = IntPtr.Zero;
			try
			{
				valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secure);
				decrypted = Marshal.PtrToStringUni(valuePtr);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
			}
			Assert.Equal(val, decrypted);
		}

		[Fact]
		public void CalculateHashTest()
		{
			string rawSource = "this is some sensitive text";
			SecureString source = rawSource.ToSecureString();
			Encoding encoding = Encoding.UTF8;
			IntPtr unmanagedString = IntPtr.Zero;
			HashAlgorithm algorithm = null;
			string saltString = "this is some salty salt";
			string result;

			try
			{
				unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(source);
				byte[] passwordBytes = encoding.GetBytes(Marshal.PtrToStringUni(unmanagedString));
				byte[] saltBytes = encoding.GetBytes(saltString);
				byte[] passwordPlusSaltBytes = new byte[passwordBytes.Length + saltBytes.Length];
				Buffer.BlockCopy(passwordBytes, 0, passwordPlusSaltBytes, 0, passwordBytes.Length);
				Buffer.BlockCopy(saltBytes, 0, passwordPlusSaltBytes, passwordBytes.Length, saltBytes.Length);
				algorithm = algorithm ?? new SHA256Managed();
				result = Convert.ToBase64String(algorithm.ComputeHash(passwordPlusSaltBytes));
			}
			finally
			{
				if (unmanagedString != IntPtr.Zero)
				{
					Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
				}
				// dispose of the algorithm
				algorithm?.Clear();
			}

			Assert.Equal(result, source.CalculateHash(saltString));
			Assert.NotEqual(result, source.CalculateHash());
		}
	}
}