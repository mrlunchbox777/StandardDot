using System;
using System.IO;

namespace StandardDot.Abstract.CoreServices
{
    /// <summary>
    /// An interface for basic serialization
    /// </summary>
    public interface ISerializationService : IDisposable
    {
        /// <summary>
        /// Serializes an object into a string
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="target">Target to serialize</param>
        /// <returns>A string representation of the object</returns>
        string SerializeObject<T>(T target);

        /// <summary>
        /// Deserializes an object from string
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="target">The string representation of the object</param>
        /// <returns>The object deserialized from the string</returns>
        T DeserializeObject<T>(string target);

        /// <summary>
        /// Deserializes an object from Stream
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="target">The Stream that contains a string representation of the object</param>
        /// <returns>The object deserialized from the Stream</returns>
        T DeserializeObject<T>(Stream target);
    }
}