using System;
using System.Collections.Generic;
using StandardDot.Abstract.DataStructures;

namespace StandardDot.Abstract.Caching
{
	/// <summary>
	/// An interface that defines a lazy caching service
	/// </summary>
	public interface ICachingService : ILazyDictionary<string, ICachedObjectBasic>
	{
		TimeSpan DefaultCacheLifespan { get; }

		/// <summary>
		/// Caches a wrapped object
		/// </summary>
		/// <typeparam name="T">The type of object to be cached</typeparam>
		/// <param name="key">The key that identifies the object</param>
		/// <param name="value">The wrapped object to cache</param>
		void Cache<T>(string key, ICachedObject<T> value);

		/// <summary>
		/// Wraps an object then caches it
		/// </summary>
		/// <typeparam name="T">The type of object to be cached</typeparam>
		/// <param name="key">The key that identifies the object</param>
		/// <param name="value">The object to cache</param>
		/// <param name="cachedTime">When the object was cached, default UTC now</param>
		/// <param name="expireTime">When the object should expire, default default cache time</param>
		void Cache<T>(string key, T value, DateTime? cachedTime = null, DateTime? expireTime = null);

		/// <summary>
		/// Gets a cached object, returns null if it is expired or doesn't exist
		/// </summary>
		/// <typeparam name="T">The type of object to be cached</typeparam>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>The wrapped cached object</returns>
		ICachedObject<T> Retrieve<T>(string key);

		/// <summary>
		/// Gets a subset of the cache that matches the key
		/// </summary>
		/// <typeparam name="T">The type of object that will be queried for</typeparam>
		/// <param name="key">The key that identifies the subset</param>
		/// <returns>A cache that represents the subset</returns>
		ICachingService Query<T>(string key);

		/// <summary>
		/// Invalidates an object in the cache
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>If it successfully invalidated the object</returns>
		bool Invalidate(string key);
	}
}
