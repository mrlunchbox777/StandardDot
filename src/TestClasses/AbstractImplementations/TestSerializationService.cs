using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using StandardDot.Abstract.CoreServices;
using StandardDot.Enums;

namespace StandardDot.TestClasses
{
    /// <summary>
    /// A Basic Json Serializer
    /// </summary>
    public class TestSerializationService : ISerializationService
    {
        /// <summary>
        /// Deserializes an object from Json string
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="target">The Json string representation of the object</param>
        /// <returns>The object deserialized from the Json string</returns>
        public T DeserializeObject<T>(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return default(T);
            }
            return DeserializeJson<T>(target);
        }

        /// <summary>
        /// Deserializes an object from Json Stream
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="target">The Json Stream that contains a string representation of the object</param>
        /// <returns>The object deserialized from the Json Stream</returns>
        public T DeserializeObject<T>(Stream target)
        {
            if (target == null)
            {
                return default(T);
            }
            return DeserializeJson<T>(GetString(target));
        }

        public void Dispose()
        {
            // nothing to dispose of
        }

        /// <summary>
        /// Serializes an object into a Json string
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="target">Target to serialize</param>
        /// <returns>A Json string representation of the object</returns>
        public string SerializeObject<T>(T target)
        {
            if (target == null)
            {
                return "";
            }
            return SerializeJson(target);
        }

        /// <summary>
        /// Serializes an object to json if possible
        /// </summary>
        /// <param name="target">The object to serialize</param>
        /// <returns>The serialized json string representation of an object</returns>
        /// <exception cref="System.Exception">
        /// Thrown when the serialization fails, and will be the type that serializer throws. Not thrown if <c>throwOnFail</c> is false.
        /// </exception>
        private static string SerializeJson<T>(T target, ILoggingService loggingService = null, bool throwOnFail = true)
        {
            if (target == null)
            {
                return string.Empty;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(T));
                    ds.WriteObject(stream, target);
                    string jsonString = GetString(stream);
                    stream.Close();
                    return jsonString;
                }
            }
            catch (Exception ex)
            {
                // if it wasn't serializable, that's ok, we expected as much
                if (!IsSerializable(target))
                {
                    loggingService?.LogMessage("Attempted serialization failed",
                        "Attempted to serialize " + target.GetType().Name + ", but it's not serializable.",
                        LogLevel.Warning);
                    if (throwOnFail)
                    {
                        throw;
                    }
                    return string.Empty;
                }

                loggingService?.LogException(ex, "ObjectExtensions.SerializeJson Exception. Target type - " + target.GetType());
                if (throwOnFail)
                {
                    throw;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks if an object is serializable
        /// </summary>
        /// <param name="target">The object to see if it is serializable</param>
        /// <returns>If the object is serializable</returns>
        private static bool IsSerializable<T>(T target)
        {
            return target is ISerializable || Attribute.IsDefined(typeof(T), typeof(SerializableAttribute))
                   || (Attribute.IsDefined(typeof(T), typeof(DataContractAttribute)));
        }

        /// <summary>
        /// Deserializes a json string to an object
        /// </summary>
        /// <param name="jsonString">The json string representation of an object.</param>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <returns>The object represented by the jsonString.</returns>
        private static T DeserializeJson<T>(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return default(T);
            }
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            T obj;
            using (Stream stream = ToStream(jsonString))
            {
                obj = (T)ser.ReadObject(stream);
            }
            return obj;
        }

        /// <summary>
        /// Converts a stream to a string. If a stream is readable, it goes to the beginning.
        /// </summary>
        /// <param name="target">The stream to convert.</param>
        /// <returns>The string pulled from the stream.</returns>
        public static string GetString(Stream target)
        {
            if (target.CanSeek)
            {
                target.Position = 0;
            }
            using (StreamReader reader = new StreamReader(target))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Converts a string to a <see cref="Stream" />.
        /// </summary>
        /// <param name="target">The string to convert to a stream.</param>
        /// <param name="encoding">The encoding for the string, default UTF8.</param>
        /// <returns>The stream created from a string.</returns>
        private static Stream ToStream(string target, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return new MemoryStream(new byte[]{});
            }
            byte[] bytes = GetBytes(target, encoding);
            return new MemoryStream(bytes);
        }

        /// <summary>
        /// Converts a string to a byte[].
        /// </summary>
        /// <param name="target">The string to convert to a byte[].</param>
        /// <param name="encoding">The encoding for the string, default UTF8.</param>
        /// <returns>The byte[] created from a string.</returns>
        private static byte[] GetBytes(string target, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetBytes(target);
        }
    }
}