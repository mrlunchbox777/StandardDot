using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace shoellibraries.CoreExtensions
{
    /// <summary>
    /// Extensions for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Deserializes a json string to an object
        /// </summary>
        /// <param name="jsonString">The json string representation of an object.</param>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <returns>The object represented by the jsonString.</returns>
        public static T DeserializeJson<T>(this string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return default(T);
            }
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            T obj;
            using (Stream stream = jsonString.GetStreamFromString())
            {
                obj = (T)ser.ReadObject(stream);
            }
            return obj;
        }

        /// <summary>
        /// Converts a string to a <see cref="Stream" />.
        /// </summary>
        /// <param name="target">The string to convert to a stream.</param>
        /// <param name="encoding">The encoding for the string, default UTF8.</param>
        /// <returns>The stream created from a string.</returns>
        public static Stream GetStreamFromString(this string target, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return new MemoryStream(new byte[]{});
            }
            byte[] bytes = target.GetBytes(encoding);
            return new MemoryStream(bytes);
        }

        /// <summary>
        /// Converts a string to a byte[].
        /// </summary>
        /// <param name="target">The string to convert to a byte[].</param>
        /// <param name="encoding">The encoding for the string, default UTF8.</param>
        /// <returns>The byte[] created from a string.</returns>
        public static byte[] GetBytes(this string target, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetBytes(target);
        }

        /// <summary>
        /// Convert a byte[] to a string.
        /// <b>NOTE: Only for use when you know the byte[] is human text</b>
        /// </summary>
        /// <param name="target">The byte[] to convert to a string</param>
        /// <param name="encoding">The encoding for the string, default UTF8.</param>
        /// <returns>The string pulled from a byte[]</returns>
        public static string GetString(this byte[] target, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetString(target);
        }

        /// <summary>
        /// Convert a byte[] to a string.
        /// <b>For use when the byte[] could be anything</b>
        /// </summary>
        /// <param name="target">The byte[] to convert to a string</param>
        /// <param name="encoding">The encoding for the string, default UTF8.</param>
        /// <returns>The base64 encoded string pulled from a byte[]</returns>
        public static string GetArbitraryString(this byte[] target)
        {
            return Convert.ToBase64String(target);
        }

        /// <summary>
        /// Decodes a base64 string
        /// </summary>
        /// <param name="base64EncodedData">A base64 encoded string</param>
        /// <returns>The decoded representation of a string</returns>
        public static string Base64Decode(this string base64EncodedData)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Encodes a string with base64
        /// </summary>
        /// <param name="plainText">A string to encode with base64</param>
        /// <returns>The base64 encoded representation of a string</returns>
        public static string Base64Encode(this string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Gets a UTC DateTime from a string that contains a UTC Unix Timestamp
        /// </summary>
        /// <param name="unixTimestamp">The string representation of a UTC Unix Timestamp</param>
        /// <returns>The UTC DateTime equivallent of the Unix Timestamp</returns>
        public static DateTime? GetDateTimeFromUnixTimestampString(this string unixTimestamp)
        {
            TimeSpan? timespanUnixTimeStamp = GetTimeSpanFromUnixTimestampString(unixTimestamp);
            if (timespanUnixTimeStamp == null)
            {
                return null;
            }
            
            return shoellibraries.Constants.DateTime.UnixEpoch.Add((TimeSpan)timespanUnixTimeStamp);
        }

        /// <summary>
        /// Gets a Timespan from a string that contains a UTC Unix Timestamp
        /// </summary>
        /// <param name="unixTimestamp">The string representation of a UTC Unix Timestamp</param>
        /// <returns>The Timespan equivallent of the Unix Timestamp</returns>
        public static TimeSpan? GetTimeSpanFromUnixTimestampString(this string unixTimestamp)
        {
            if (string.IsNullOrWhiteSpace(unixTimestamp))
            {
                return null;
            }
            
            ulong unixTimestampSeconds = Convert.ToUInt64(unixTimestamp);

            return TimeSpan.FromSeconds(unixTimestampSeconds);}
    }
}