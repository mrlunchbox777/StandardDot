using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace StandardDot.CoreExtensions
{
	/// <summary>
	/// Extensions for SecureString.
	/// </summary>
	public static class SecureStringExtensions
	{
		/// <summary>
		/// Converts a <see cref="SecureString" /> to a string
		/// </summary>
		/// <param name="source">The <see cref="SecureString" /></param>
		public static string ToPlainText(this SecureString source)
		{
			IntPtr valuePtr = IntPtr.Zero;
			try
			{
				valuePtr = Marshal.SecureStringToGlobalAllocUnicode(source);
				return Marshal.PtrToStringUni(valuePtr);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
			}
		}

		/// <summary>
		/// Gets the hash of a <see cref="SecureString" />
		/// </summary>
		/// <param name="source">The <see cref="SecureString" /> to get the hash of</param>
		/// <param name="saltString">The salt used for secure strings</param>
		/// <param name="algorithm">The algorithm used, default <see cref="SHA256Managed" /></param>
		/// <param name="encoding">The encoding used in the secured string and salt, default <see cref="Encoding.UTF8" /></param>
		/// <returns>A Hash representation of the SecureString</returns>
		public static string CalculateHash(this SecureString source, string saltString = null, HashAlgorithm algorithm = null, Encoding encoding = null)
		{
			encoding = encoding ?? Encoding.UTF8;
			IntPtr unmanagedString = IntPtr.Zero;
			try
			{
				unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(source);
				byte[] allBytes;
				byte[] passwordBytes = encoding.GetBytes(Marshal.PtrToStringUni(unmanagedString));
				if (!string.IsNullOrWhiteSpace(saltString))
				{
					byte[] saltBytes = encoding.GetBytes(saltString);
					byte[] passwordPlusSaltBytes = new byte[passwordBytes.Length + saltBytes.Length];
					Buffer.BlockCopy(passwordBytes, 0, passwordPlusSaltBytes, 0, passwordBytes.Length);
					Buffer.BlockCopy(saltBytes, 0, passwordPlusSaltBytes, passwordBytes.Length, saltBytes.Length);
					allBytes = passwordPlusSaltBytes;
				}
				else
				{
					allBytes = passwordBytes;
				}
				algorithm = algorithm ?? new SHA256Managed();
				return Convert.ToBase64String(algorithm.ComputeHash(allBytes));
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
		}
	}
}