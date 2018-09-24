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
			if ((!(serializationSettings?.KnownTypes?.Any() ?? false)) && (serializationSettings?.Resolver == null))
			{
				ser = new DataContractJsonSerializer(typeof(T));
			}
			else if (serializationSettings?.Resolver != null)
			{
				throw new InvalidOperationException("Json serialization doesn't use a resolver.");
			}
			else if (serializationSettings?.KnownTypes?.Any() ?? false)
			{
				ser = new DataContractJsonSerializer(typeof(T), serializationSettings.KnownTypes);
			}
			return ser;
		}
	}
}