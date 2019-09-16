using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace StandardDot.Abstract.CoreServices
{
	public interface ISerializationSettings
	{
		List<Type> KnownTypes { get; }

		DataContractResolver Resolver { get; }
	}
}