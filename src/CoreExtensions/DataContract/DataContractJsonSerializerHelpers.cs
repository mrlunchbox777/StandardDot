using System;
using System.Linq;
using System.Runtime.Serialization.Json;
using StandardDot.Abstract.CoreServices;

namespace CoreExtensions.DataContract
{
	public static class DataContractJsonSerializerHelpers
	{
		public static DataContractJsonSerializer GetSerializer<T>(ISerializationSettings dataContractResolver)
		{
			DataContractJsonSerializer ser = null;
			if ((!(dataContractResolver?.KnownTypes?.Any() ?? false)) && (dataContractResolver?.Resolver == null))
			{
				ser = new DataContractJsonSerializer(typeof(T));
			}
			else if (dataContractResolver?.Resolver != null)
			{
				throw new InvalidOperationException("Json serialization doesn't use a resolver.");
			}
			else if (dataContractResolver?.KnownTypes?.Any() ?? false)
			{
				ser = new DataContractJsonSerializer(typeof(T), dataContractResolver.KnownTypes);
			}
			return ser;
		}
	}
}