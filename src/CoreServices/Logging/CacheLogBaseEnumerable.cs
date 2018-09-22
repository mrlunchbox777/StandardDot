using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;

namespace StandardDot.CoreServices.Logging
{
    /// <summary>
    /// An Enumerable to get text logs
    /// </summary>
    public class CacheLogBaseEnumerable : LogBaseEnumerableBase
    {
        /// <param name="source">The source that the enumerable should represent</param>
        public CacheLogBaseEnumerable(IEnumerable<LogBase> source)
            : base(source)
        { }

        /// <param name="source">The source that the enumerable should represent</param>
        public CacheLogBaseEnumerable(ILogBaseEnumerable source)
            : base(source)
        { }

        /// <param name="cachingService">The caching service that backs the enumerable</param>
        /// <param name="serializationService">The serialization service to use</param>
        /// <param name="onlySerializeLogsOfTheCorrectType">Only serializes logs of the correct type, has a significant performance hit</param>
        public CacheLogBaseEnumerable(ICachingService cachingService, ISerializationService serializationService, bool onlySerializeLogsOfTheCorrectType = false)
            : base(null)
        {
            SerializationService = serializationService;
            CachingService = cachingService;
            OnlySerializeLogsOfTheCorrectType = onlySerializeLogsOfTheCorrectType;
        }

        protected virtual ICachingService CachingService { get; }

        protected virtual ISerializationService SerializationService { get; }

        protected virtual bool OnlySerializeLogsOfTheCorrectType { get; }

        public override IEnumerator<LogBase> GetEnumerator()
        {
            if (CachingService == null) 
            {
                return base.GetEnumerator();
            }
            return CachingService
                .Select(i => i.Value)
                .Where(i => i.ExpireTime >= DateTime.UtcNow)
                .Select(i => i.UntypedValue as LogBase)
                .Where(i => !OnlySerializeLogsOfTheCorrectType || i != null)
                .GetEnumerator();
        }
    }
}