using System;

namespace StandardDot.Abstract.Caching
{
	/// <summary>
	/// An interface that defines how objects are cached
	/// </summary>
	public interface ICachedObjectBasic
	{
		DateTime CachedTime { get; set; }

		DateTime ExpireTime { get; set; }

		object UntypedValue { get; set; }
	}
}