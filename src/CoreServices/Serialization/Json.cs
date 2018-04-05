using System;
using System.IO;
using StandardDot.CoreExtensions;
using StandardDot.CoreExtensions.Object;

namespace StandardDot.Abstract.CoreServices
{
    /// <summary>
    /// A Basic Json Serializer
    /// </summary>
    public class Json : ISerializationService
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
            return target.DeserializeJson<T>();
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
            return target.GetStringFromStream().DeserializeJson<T>();
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
            return target.SerializeJson();
        }
    }
}