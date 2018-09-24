using System;
using System.IO;
using StandardDot.Abstract.CoreServices;
using StandardDot.CoreExtensions;
using StandardDot.CoreExtensions.Object;

namespace StandardDot.CoreServices.Serialization
{
	/// <summary>
	/// A Basic Json Serializer
	/// </summary>
	public class Json : ISerializationService
	{
		public Json()
		{}

		public Json(ILoggingService loggingService)
		{
			LoggingService = loggingService;
		}

		protected virtual ILoggingService LoggingService { get; }

		/// <summary>
		/// Deserializes an object from Json string
		/// </summary>
		/// <typeparam name="T">The target type (must be serializable)</typeparam>
		/// <param name="target">The Json string representation of the object</param>
		/// <param name="settings">Serialization Settings</param>
		/// <returns>The object deserialized from the Json string</returns>
		public T DeserializeObject<T>(string target, ISerializationSettings settings = null)
		{
			if (string.IsNullOrWhiteSpace(target))
			{
				return default(T);
			}
			return target.DeserializeJson<T>(settings);
		}

		/// <summary>
		/// Deserializes an object from Json Stream
		/// </summary>
		/// <typeparam name="T">The target type (must be serializable)</typeparam>
		/// <param name="target">The Json Stream that contains a string representation of the object</param>
		/// <param name="settings">Serialization Settings</param>
		/// <returns>The object deserialized from the Json Stream</returns>
		public T DeserializeObject<T>(Stream target, ISerializationSettings settings = null)
		{
			if (target == null)
			{
				return default(T);
			}
			return target.GetString().DeserializeJson<T>(settings);
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
		/// <param name="settings">Serialization Settings</param>
		/// <returns>A Json string representation of the object</returns>
		public string SerializeObject<T>(T target, ISerializationSettings settings = null)
		{
			if (target == null)
			{
				return "";
			}
			return target.SerializeJson(LoggingService, true, settings);
		}
	}
}