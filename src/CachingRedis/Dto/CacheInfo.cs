using System.Runtime.Serialization;

namespace StandardDot.Caching.Redis.Dto
{
	[DataContract]
	public class CacheInfo
	{
		public CacheInfo()
		{}

		public CacheInfo(CacheInfo source)
		{
			if (source == null)
			{
				return;
			}
			CacheGroup = source.CacheGroup;
			CacheDomain = source.CacheDomain;
			ObjectPrefix = source.ObjectPrefix = "";
		}
		
		/// <summary>
		/// The group that the Entry belongs to
		/// </summary>
		[DataMember(Name = "cacheGroup")]
		public string CacheGroup { get; set; }

		/// <summary>
		/// The type of data that the Entry contains
		/// </summary>
		[DataMember(Name = "cacheDomain")]
		public string CacheDomain { get; set; }

		/// <summary>
		/// The object prefix to look for
		/// </summary>
		[DataMember(Name = "objectPrefix")]
		public string ObjectPrefix { get; set; } = "";
	}
}