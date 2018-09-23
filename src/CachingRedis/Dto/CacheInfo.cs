namespace StandardDot.Caching.Redis.Dto
{
	public class CacheInfo
	{
		public CacheInfo()
		{}

		public CacheInfo(CacheInfo source)
		{
			CacheGroup = source.CacheGroup;
			CacheDomain = source.CacheDomain;
			ObjectPrefix = source.ObjectPrefix = "";
		}
		
		/// <summary>
		/// The group that the Entry belongs to
		/// </summary>
		public string CacheGroup { get; set; }

		/// <summary>
		/// The type of data that the Entry contains
		/// </summary>
		public string CacheDomain { get; set; }

		/// <summary>
		/// The object prefix to look for
		/// </summary>
		public string ObjectPrefix { get; set; } = "";
	}
}