using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Abstract.CoreServices
{
	public interface IDataContractResolver
	{
		List<Type> KnownTypes { get; }

		DataContractResolver Resolver { get; }
	}
}