using System;
using System.Linq;
using System.Runtime.Serialization.Json;
using StandardDot.Abstract.CoreServices;
using StandardDot.CoreExtensions;

namespace CoreExtensions.DataContract
{
	/// <summary>
	/// Extensions for <see cref="ISerializationSettings" />
	/// </summary>
	public static class DataContractJsonSerializerHelpers
	{
		/// <summary>
		/// Creates a <see cref="DataContractJsonSerializer" />
		/// </summary>
		/// <param name="serializationSettings">The settings to use to create the <see cref="DataContractJsonSerializer" /></param>
		/// <exception cref="System.InvalidOperationException">
		/// <see cref="DataContractJsonSerializer" /> does not support a <see cref="DataContractResolver" />
		/// </exception>
		/// <returns>A new <see cref="DataContractJsonSerializer" /></returns>
		public static DataContractJsonSerializer GetSerializer<T>(ISerializationSettings serializationSettings)
		{
			if (serializationSettings == null)
			{
				return new DataContractJsonSerializer(typeof(T));
			}

			DataContractJsonSerializer ser = null;
			bool knownTypesExist = serializationSettings.KnownTypes.AnySafe();
			bool resolverExists = serializationSettings.Resolver != null;

			if (!knownTypesExist && !resolverExists)
			{
				ser = new DataContractJsonSerializer(typeof(T));
			}
			else if (resolverExists)
			{
				throw new InvalidOperationException("Json serialization doesn't use a resolver.");
			}
			else if (knownTypesExist)
			{
				ser = new DataContractJsonSerializer(typeof(T), serializationSettings.KnownTypes);
			}

			return ser;
		}
	}
}