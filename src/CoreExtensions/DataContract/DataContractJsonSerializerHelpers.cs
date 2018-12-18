using System;
using System.Linq;
using System.Runtime.Serialization.Json;
using StandardDot.Abstract.CoreServices;

namespace CoreExtensions.DataContract
{
	public static class DataContractJsonSerializerHelpers
	{
		public static DataContractJsonSerializer GetSerializer<T>(ISerializationSettings serializationSettings)
		{
			DataContractJsonSerializer ser = null;
			bool knownTypesExist = serializationSettings?.KnownTypes?.Any() ?? false;
			bool resolverExists = serializationSettings?.Resolver != null;
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