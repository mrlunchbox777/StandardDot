using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.Abstract.DataStructures;

namespace StandardDot.TestClasses.AbstractImplementations
{
	public class TestMemoryCachingService : ICachingService
	{
		/// <param name="defaultCacheLifespan">How long items should be cached by default</param>
		/// <param name="cache">The cache to use, default is a thread safe dictionary</param>
		public TestMemoryCachingService(TimeSpan defaultCacheLifespan, IDictionary<string, ICachedObjectBasic> cache = null)
		{
			DefaultCacheLifespan = defaultCacheLifespan;
			Store = cache ?? new ConcurrentDictionary<string, ICachedObjectBasic>();
		}

		/// <param name="defaultCacheLifespan">How long items should be cached by default</param>
		/// <param name="useStaticCache">If this instance should use a static cache (thread safe)</param>
		public TestMemoryCachingService(TimeSpan defaultCacheLifespan, bool useStaticCache)
		{
			DefaultCacheLifespan = defaultCacheLifespan;
			Store = useStaticCache ? _store : new ConcurrentDictionary<string, ICachedObjectBasic>();
		}

		private static IDictionary<string, ICachedObjectBasic> _store = new ConcurrentDictionary<string, ICachedObjectBasic>();

		protected virtual IDictionary<string, ICachedObjectBasic> Store { get; }

		/// <summary>
		/// Wraps an object for caching
		/// </summary>
		/// <typeparam name="T">The configuration type</typeparam>
		/// <param name="value">The object to wrap</param>
		/// <param name="cachedTime">When the object was cached, default UTC now</param>
		/// <param name="expireTime">When the object should expire, default UTC now + DefaultCacheLifespan</param>
		/// <returns>The wrapped object</returns>
		protected virtual ICachedObject<T> CreateCachedObject<T>(T value, DateTime? cachedTime = null, DateTime? expireTime = null)
		{
			return new TestDefaultCachedObject<T>
			{
				Value = value,
				CachedTime = cachedTime ?? DateTime.UtcNow,
				ExpireTime = expireTime ?? DateTime.UtcNow.Add(DefaultCacheLifespan)
			};
		}

		public virtual TimeSpan DefaultCacheLifespan { get; }

		ICollection<string> IDictionary<string, ICachedObjectBasic>.Keys => Keys;

		ICollection<ICachedObjectBasic> IDictionary<string, ICachedObjectBasic>.Values => Values;

		public int Count => Store.Count;

		public bool IsReadOnly => Store.IsReadOnly;

		public ILazyCollection<string> Keys => new LazyCollectionWrapper<string>(Store.Keys);

		public ILazyCollection<ICachedObjectBasic> Values => new LazyCollectionWrapper<ICachedObjectBasic>(Store.Values);

		/// <summary>
		/// Gets an object from cache, null if not found
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>The cached wrapped object, default null</returns>
		public ICachedObjectBasic this[string key]
		{
			get => Retrieve<object>(key);
			set => Cache<object>(key, value);
		}

		/// <summary>
		/// Caches an object, overwrites it if it is already cached
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <param name="value">The wrapped object to cache</param>
		public virtual void Cache<T>(string key, ICachedObject<T> value)
		{
			if (value == null)
			{
				return;
			}
			if (ContainsKey(key))
			{
				Invalidate(key);
			}
			Store.Add(key, CreateCachedObject((object)value.Value, value.CachedTime, value.ExpireTime));
		}

		/// <summary>
		/// Caches an object, overwrites it if it is already cached
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <param name="value">The object to cache</param>
		/// <param name="cachedTime">The time the object was cached, default UTC now</param>
		/// <param name="expireTime">When the object should expire, default UTC now + DefaultCacheLifespan</param>
		public void Cache<T>(string key, T value, DateTime? cachedTime = null, DateTime? expireTime = null)
		{
			Cache<T>(key, CreateCachedObject(value, cachedTime, expireTime));
		}

		/// <summary>
		/// Gets an object from cache, null if not found
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>The cached wrapped object, default null</returns>
		public ICachedObject<T> Retrieve<T>(string key)
		{
			if (!ContainsKey(key))
			{
				return null;
			}

			ICachedObjectBasic item = Store[key];
			if (!(item.UntypedValue is T))
			{
				return null;
			}
			T result = (T)item.UntypedValue;
			if (item.ExpireTime < DateTime.UtcNow)
			{
				Invalidate(key);
				return null;
			}
			return CreateCachedObject<T>(result, item.CachedTime, item.ExpireTime);
		}

		/// <summary>
		/// Removes an object from the cache
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>If the object was able to be removed</returns>
		public bool Invalidate(string key)
		{
			if (ContainsKey(key))
			{
				return Store.Remove(key);
			}
			return false;
		}

		/// <summary>
		/// Caches an object, overwrites it if it is already cached
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <param name="value">The wrapped object to cache</param>
		public void Add(string key, ICachedObjectBasic value)
		{
			Cache<object>(key, value);
		}

		/// <summary>
		/// Checks if an object exists in the cache and is valid
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>If the object was found and valid</returns>
		public bool ContainsKey(string key)
		{
			return Store.ContainsKey(key);
		}

		/// <summary>
		/// Removes an object from the cache
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>If the object was able to be removed</returns>
		public bool Remove(string key)
		{
			return Invalidate(key);
		}

		/// <summary>
		/// Gets an object from cache, null if not found
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <param name="value">The wrapped object to cache</param>
		/// <returns>If the value was able to be retrieved</returns>
		public bool TryGetValue(string key, out ICachedObjectBasic value)
		{
			value = Retrieve<object>(key);
			return value != null;
		}

		/// <summary>
		/// Caches an object, overwrites it if it is already cached
		/// </summary>
		/// <param name="item">(The key that identifies the object, The wrapped object to cache)</param>
		public void Add(KeyValuePair<string, ICachedObjectBasic> item)
		{
			Cache(item.Key, item.Value);
		}

		/// <summary>
		/// Clears the cache of all cached items.
		/// </summary>
		public void Clear()
		{
			Store.Clear();
		}

		/// <summary>
		/// Checks if an object exists in the cache and is valid
		/// </summary>
		/// <param name="item">(The key that identifies the object, The wrapped object to cache)</param>
		/// <returns>If the object was found and valid</returns>
		public bool Contains(KeyValuePair<string, ICachedObjectBasic> item)
		{
			if (!ContainsKey(item.Key))
			{
				return false;
			}

			ICachedObjectBasic value = Store[item.Key];

			if (value.UntypedValue == null)
			{
				return false;
			}
			if (value.ExpireTime < DateTime.UtcNow)
			{
				Invalidate(item.Key);
				return false;
			}
			if (value.CachedTime != item.Value.CachedTime)
			{
				return false;
			}
			if (value.ExpireTime != item.Value.ExpireTime)
			{
				return false;
			}
			if (value.UntypedValue != item.Value.UntypedValue)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Copies the cache to an array
		/// </summary>
		/// <param name="array">The destination of the copy</param>
		/// <param name="arrayIndex">Where to start the copy at in the destination</param>
		public void CopyTo(KeyValuePair<string, ICachedObjectBasic>[] array, int arrayIndex)
		{
			Store.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes an object from the cache
		/// </summary>
		/// <param name="item">(The key that identifies the object, The wrapped object to cache)</param>
		/// <returns>If the object was able to be removed</returns>
		public bool Remove(KeyValuePair<string, ICachedObjectBasic> item)
		{
			if (!ContainsKey(item.Key))
			{
				return false;
			}

			ICachedObjectBasic value = Store[item.Key];

			if (value.UntypedValue == null)
			{
				return false;
			}
			if (value.ExpireTime < DateTime.UtcNow)
			{
				Invalidate(item.Key);
				return false;
			}
			if (value.CachedTime != item.Value.CachedTime)
			{
				return false;
			}
			if (value.ExpireTime != item.Value.ExpireTime)
			{
				return false;
			}
			if (value.UntypedValue != item.Value.UntypedValue)
			{
				return false;
			}
			return Store.Remove(item.Key);
		}

		/// <summary>
		/// Gets the typed enumerator for the cache
		/// </summary>
		/// <returns>The typed enumerator</returns>
		public IEnumerator<KeyValuePair<string, ICachedObjectBasic>> GetEnumerator()
		{
			return Store.GetEnumerator();
		}

		/// <summary>
		/// Gets the enumerator for the cache
		/// </summary>
		/// <returns>The enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return Store.GetEnumerator();
		}

		public IDictionary<string, ICachedObjectBasic> EnumerateDictionary()
		{
			return this;
		}
		
		public ICachingService Query<T>(string key)
		{
			return new TestMemoryCachingService(DefaultCacheLifespan, Store
				.Where(x => (key == null && x.Key == null) || (x.Key?.StartsWith(key) ?? false))
				.ToDictionary(x => x.Key, x => x.Value)
			);
		}
	}
}