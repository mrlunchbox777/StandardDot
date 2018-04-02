using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using shoellibraries.Enums;
using shoellibraries.Abstract;
using shoellibraries.CoreExtensions;

namespace shoellibraries.CoreExtensions.Object
{
    /// <summary>
    /// Serialization Extensions for objects
    /// </summary>
    public static class ObjectSerializationExtensions
    {
        /// <summary>
        /// Serializes an object to json if possible
        /// </summary>
        /// <param name="target">The object to serialize</param>
        /// <returns>The serialized json string representation of an object</returns>
        /// <exception cref="System.Exception">
        /// Thrown when the serialization fails, and will be the type that serializer throws. Not thrown if <c>throwOnFail</c> is false.
        /// </exception>
        public static string SerializeJson<T>(this T target, ILoggingService loggingService = null, bool throwOnFail = true)
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
                    string jsonString = stream.GetStringFromStream();
                    stream.Close();
                    return jsonString;
                }
            }
            catch (Exception ex)
            {
                // if it wasn't serializable, that's ok, we expected as much
                if (!target.IsSerializable())
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
        private static bool IsSerializable<T>(this T target)
        {
            return target is ISerializable || Attribute.IsDefined(typeof(T), typeof(SerializableAttribute))
                   || (Attribute.IsDefined(typeof(T), typeof(DataContractAttribute)));
        }
    }
}