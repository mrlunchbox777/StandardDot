namespace StandardDot.Caching.Redis.Dto
{
	public class CacheInfo
	{
		/// <summary>
		/// The group that the Entry belongs to
		/// </summary>
		public string CacheGroup { get; set; }

		/// <summary>
		/// The type of data that the Entry contains
		/// </summary>
		public string CacheDomain { get; set; }
	}
}