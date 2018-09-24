using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.Abstract.DataStructures;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.DataStructures;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Providers;
using StandardDot.Caching.Redis.Service;
using StandardDot.CoreServices.Extensions;

namespace StandardDot.Caching.Redis
{
	public class RedisCachingService : ICachingService
	{
		/// <param name="defaultCacheLifespan">How long items should be cached by default</param>
		/// <param name="cache">The cache to use, default is a thread safe dictionary</param>
		public RedisCachingService(ICacheProviderSettings settings, ILoggingService loggingService, CacheInfo cacheInfo = null, bool useStaticProvider = true)
		{
			_settings = settings;
			_cacheInfo = cacheInfo ?? _settings.ServiceSettings.ProviderInfo;
			_loggingService = loggingService;
			UseStaticProvider = useStaticProvider;
		}

		protected virtual RedisId GetRedisId(RedisId key)
		{
			return key;
		}

		protected virtual RedisId GetRedisId(string key, bool tryJson = true)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return null;
			}
			if (tryJson && key.StartsWith("{") && key.EndsWith("}")
				&& RedisId.DataMemberNames.All(x => key.Contains(x)))
			{
				return key;
			}
			return new RedisId
			{
				ServiceType = _settings.ServiceSettings.RedisServiceImplementationType,
				ObjectIdentifier = _cacheInfo.ObjectPrefix + key,
				HashSetIdentifier = _settings.ServiceSettings.PrefixIdentifier
			};
		}

		protected virtual bool UseStaticProvider { get; }

		protected virtual ICacheProviderSettings _settings { get; }

		protected virtual CacheInfo _cacheInfo { get; }

		private ILoggingService _loggingService;

		public ILoggingService LoggingService => _loggingService;

		private static ConcurrentDictionary<Guid, RedisService> _store
			= new ConcurrentDictionary<Guid, RedisService>();

		private RedisService Store
		{
			get
			{
				RedisService provider;
				if (UseStaticProvider && _store.ContainsKey(_settings.ServiceSettings.CacheProviderSettingsId))
				{
					provider = _store[_settings.ServiceSettings.CacheProviderSettingsId];
					if (_settings == provider.CacheSettings)
					{
						return provider;
					}
				}
				provider = new RedisService(_settings, LoggingService, (s, l) => new RedisCacheProvider(s, l));
				if (UseStaticProvider)
				{
					_store[_settings.ServiceSettings.CacheProviderSettingsId] = provider;
				}
				return provider;
			}
		}

		/// <summary>
		/// Wraps an object for caching
		/// </summary>
		/// <typeparam name="T">The configuration type</typeparam>
		/// <param name="key">The key that identifies the object</param>
		/// <param name="value">The object to wrap</param>
		/// <param name="cachedTime">When the object was cached, default UTC now</param>
		/// <param name="expireTime">When the object should expire, default UTC now + DefaultCacheLifespan</param>
		/// <returns>The wrapped object</returns>
		protected virtual RedisCachedObject<T> CreateCachedObject<T>(RedisId key, T value, DateTime? cachedTime = null, DateTime? expireTime = null)
		{
			return new RedisCachedObject<T>(key)
			{
				Value = value,
				CachedTime = cachedTime ?? DateTime.UtcNow,
				ExpireTime = expireTime ?? DateTime.UtcNow.Add(DefaultCacheLifespan),
				Metadata = _cacheInfo
			};
		}

		public virtual TimeSpan DefaultCacheLifespan => _settings.ServiceSettings.DefaultExpireTimeSpan ?? TimeSpan.FromSeconds(300);

		// probably pretty slow
		public ILazyCollection<string> Keys
			=> new RedisLazyCollection<string>(Store.RedisServiceImplementation
				.GetKeys<object>(new List<RedisId> { GetRedisId("*", false) }).Select(x => (string)x), this);

		// probably pretty slow
		public ILazyCollection<ICachedObjectBasic> Values
			=> new RedisLazyCollection<ICachedObjectBasic>(Store.RedisServiceImplementation
				.GetValues<object>(new List<RedisId> { GetRedisId("*", false) }), this);

		ICollection<string> IDictionary<string, ICachedObjectBasic>.Keys => Keys.ToList();

		ICollection<ICachedObjectBasic> IDictionary<string, ICachedObjectBasic>.Values => Values.ToList();

		// probably pretty slow
		public int Count
		{
			get
			{
				long count = Store.KeyCount();
				if (count > int.MaxValue)
				{
					return 0;
				}
				return (int)count;
			}
		}

		public bool IsReadOnly => false;

		/// <summary>
		/// Gets an object from cache, null if not found
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>The cached wrapped object, default null</returns>
		public ICachedObjectBasic this[string key]
		{
			get => Retrieve<object>(GetRedisId(key));
			set => Cache<object>(GetRedisId(key), value);
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
			RedisId id = GetRedisId(key);
			if (ContainsKey(id))
			{
				Invalidate(id);
			}
			Store.SetValue(CreateCachedObject(id, value.Value, value.CachedTime, value.ExpireTime));
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
			RedisId id = GetRedisId(key);
			Cache<T>(id, CreateCachedObject(id, value, cachedTime, expireTime));
		}

		/// <summary>
		/// Gets an object from cache, null if not found
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>The cached wrapped object, default null</returns>
		public ICachedObject<T> Retrieve<T>(string key)
		{
			RedisId id = GetRedisId(key);
			if (!ContainsKey(id))
			{
				return null;
			}

			ICachedObject<T> item = Store.GetValue<T>(id).SingleOrDefault();
			if (!(item.Value is T))
			{
				return null;
			}
			T result = (T)item.Value;
			if (item.ExpireTime < DateTime.UtcNow)
			{
				Invalidate(id);
				return null;
			}
			return item;
		}

		/// <summary>
		/// Removes an object from the cache
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>If the object was able to be removed</returns>
		public bool Invalidate(string key)
		{
			RedisId id = GetRedisId(key);
			if (ContainsKey(id))
			{
				Store.DeleteValue(id);
				return true;
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
			RedisId id = GetRedisId(key);
			Cache<object>(id, value.UntypedValue, value.CachedTime, value.ExpireTime);
		}

		/// <summary>
		/// Checks if an object exists in the cache and is valid
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>If the object was found and valid</returns>
		public bool ContainsKey(string key)
		{
			RedisId id = GetRedisId(key);
			return Store.ContainsKey(id);
		}

		/// <summary>
		/// Removes an object from the cache
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <returns>If the object was able to be removed</returns>
		public bool Remove(string key)
		{
			RedisId id = GetRedisId(key);
			return Store.DeleteValue(id);
		}

		/// <summary>
		/// Gets an object from cache, null if not found
		/// </summary>
		/// <param name="key">The key that identifies the object</param>
		/// <param name="value">The wrapped object to cache</param>
		/// <returns>If the value was able to be retrieved</returns>
		public bool TryGetValue(string key, out ICachedObjectBasic value)
		{
			RedisId id = GetRedisId(key);
			value = Retrieve<object>(id);
			return value != null;
		}

		/// <summary>
		/// Caches an object, overwrites it if it is already cached
		/// </summary>
		/// <param name="item">(The key that identifies the object, The wrapped object to cache)</param>
		public void Add(KeyValuePair<string, ICachedObjectBasic> item)
		{
			RedisId id = GetRedisId(item.Key);
			Cache(id, item.Value);
		}

		/// <summary>
		/// Clears the cache of all cached items.
		/// </summary>
		public void Clear()
		{
			Store.DeleteValue(GetRedisId("*", false));
		}

		/// <summary>
		/// Checks if an object exists in the cache and is valid
		/// </summary>
		/// <param name="item">(The key that identifies the object, The wrapped object to cache)</param>
		/// <returns>If the object was found and valid</returns>
		public bool Contains(KeyValuePair<string, ICachedObjectBasic> item)
		{
			if (string.IsNullOrWhiteSpace(item.Key))
			{
				return false;
			}
			RedisId id = GetRedisId(item.Key);
			if (!ContainsKey(id))
			{
				return false;
			}

			ICachedObject<object> value = Store.GetValue<object>(id).SingleOrDefault();

			if (value?.Value == null)
			{
				return false;
			}
			if (value.ExpireTime < DateTime.UtcNow)
			{
				Invalidate(id);
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
			ISerializationService service = Store.GetSerializationService<object>();
			if (service.SerializeObject(value.Value) != service.SerializeObject(item.Value.UntypedValue, _settings.SerializationSettings))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Copies the cache to an array. This will be very slow.
		/// </summary>
		/// <param name="array">The destination of the copy</param>
		/// <param name="arrayIndex">Where to start the copy at in the destination</param>
		public void CopyTo(KeyValuePair<string, ICachedObjectBasic>[] array, int arrayIndex)
		{
			KeyValuePair<string, ICachedObject<object>>[] localStore =
				Store.GetValue<object>(GetRedisId("*", false))
				.Select(x => new KeyValuePair<string, ICachedObject<object>>(x.Id, x))
				.ToArray();
			localStore.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes an object from the cache
		/// </summary>
		/// <param name="item">(The key that identifies the object, The wrapped object to cache)</param>
		/// <returns>If the object was able to be removed</returns>
		public bool Remove(KeyValuePair<string, ICachedObjectBasic> item)
		{
			if (!Contains(item))
			{
				return false;
			}
			RedisId id = GetRedisId(item.Key);
			return Store.DeleteValue(id);
		}

		/// <summary>
		/// Gets the typed enumerator for the cache
		/// </summary>
		/// <returns>The typed enumerator</returns>
		public IEnumerator<KeyValuePair<string, ICachedObjectBasic>> GetEnumerator()
		{
			return Store.GetValue<object>(GetRedisId("*", false))
				.Select(x => new KeyValuePair<string, ICachedObjectBasic>(x.Id, x))
				.GetEnumerator();
		}

		/// <summary>
		/// Gets the enumerator for the cache
		/// </summary>
		/// <returns>The enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IDictionary<string, ICachedObjectBasic> EnumerateDictionary()
		{
			return this.ToDictionary(x => x.Key, x => x.Value);
		}

		public ICachingService Query<T>(string key)
		{
			return new RedisCachingService(_settings, LoggingService,
				new CacheInfo(_cacheInfo)
				{
					ObjectPrefix = _cacheInfo.ObjectPrefix + key
				}, UseStaticProvider
			);
		}
	}
}