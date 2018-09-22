using System;
using System.IO;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;

namespace StandardDot.CoreServices.Logging
{
	/// <summary>
	/// A Text File Logging Service
	/// </summary>
	public class CacheLoggingService : LoggingServiceBase
	{
		/// <param name="cachingService">The service to cache the logs to</param>
		/// <param name="serializationService">The serialization service to use</param>
		public CacheLoggingService(ICachingService cachingService, ISerializationService serializationService, string cacheItemNamePartSeparator = "_")
			: base(serializationService)
		{
			CachingService = cachingService;
			CacheItemNamePartSeparator = cacheItemNamePartSeparator;
		}

		protected virtual ICachingService CachingService { get; set; }

		protected virtual string CacheItemNamePartSeparator { get; set; }

		/// <summary>
		/// Logs a log object
		/// </summary>
		/// <typeparam name="T">The target type (must be serializable)</typeparam>
		/// <param name="log">The log to store</param>
		public override void Log<T>(Log<T> log)
		{
			string uniqueId = Guid.NewGuid().ToString("N");
			string logName = log.LogLevel + CacheItemNamePartSeparator + log.TimeStamp.ToFileTimeUtc() + CacheItemNamePartSeparator + uniqueId;
			string serializedLog = SerializationService.SerializeObject(log);
			CachingService.Cache(logName, log);
		}

		/// <summary>
		/// Get object to find all logs (can be filtered)
		/// </summary>
		/// <typeparam name="T">The target type for the logs (must be serializable)</typeparam>
		/// <returns>IEnumerable that iterates through all logs</returns>
		protected override LogEnumerableBase<T> BaseGetLogs<T>()
		{
			return new CacheLogEnumerable<T>(CachingService, SerializationService, true);
		}

		/// <summary>
		/// Get object to find all logs (can be filtered)
		/// </summary>
		/// <returns>IEnumerable that iterates through all logs</returns>
		protected override ILogBaseEnumerable BaseGetLogs()
		{
			return new CacheLogBaseEnumerable(CachingService, SerializationService);
		}
	}
}