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
	/// <typeparam name="T">The target type for the logs (must be serializable)</typeparam>
	public class CacheLogEnumerable<T> : LogEnumerableBase<T>
		where T : new()
	{
		/// <param name="source">The source that the enumerable should represent</param>
		public CacheLogEnumerable(IEnumerable<Log<T>> source)
			: base(source)
		{ }

		/// <param name="source">The source that the enumerable should represent</param>
		public CacheLogEnumerable(ILogEnumerable<T> source)
			: base(source)
		{ }

		/// <param name="cachingService">The caching service that backs the enumerable</param>
		/// <param name="serializationService">The serialization service to use</param>
		/// <param name="onlySerializeLogsOfTheCorrectType">Only serializes logs of the correct type, has a significant performance hit</param>
		public CacheLogEnumerable(ICachingService cachingService, ISerializationService serializationService, bool onlySerializeLogsOfTheCorrectType = false)
			: base(null)
		{
			SerializationService = serializationService;
			CachingService = cachingService;
			OnlySerializeLogsOfTheCorrectType = onlySerializeLogsOfTheCorrectType;
		}

		protected virtual ICachingService CachingService { get; }

		protected virtual ISerializationService SerializationService { get; }

		protected virtual bool OnlySerializeLogsOfTheCorrectType { get; }

		public override IEnumerator<Log<T>> GetEnumerator()
		{
			if (CachingService == null)
			{
				return base.GetEnumerator();
			}
			return CachingService
				.Select(i => i.Value)
				.Where(i => i.ExpireTime >= DateTime.UtcNow)
				.Select(i => i.UntypedValue as Log<T>)
				.Where(i => !OnlySerializeLogsOfTheCorrectType || i != null)
				.GetEnumerator();
		}
	}
}