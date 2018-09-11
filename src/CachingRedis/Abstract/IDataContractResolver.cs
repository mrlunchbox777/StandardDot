using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace StandardDot.Caching.Redis.Abstract
{
    public interface IDataContractResolver
    {
        List<Type> KnownTypes { get; }

        DataContractResolver Resolver { get; }
    }
}
