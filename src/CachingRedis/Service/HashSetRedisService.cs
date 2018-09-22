using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using StackExchange.Redis;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;
using StandardDot.Abstract.CoreServices;

namespace StandardDot.Caching.Redis.Service
{
	/// <summary>
	/// Check redis service for summaries
	/// </summary>
	internal class HashSetRedisService : IRedisService
	{
		public HashSetRedisService(RedisService redisService)
		{
			RedisService = redisService;
		}

		public RedisService RedisService { get; }

		public RedisServiceType ServiceType => RedisServiceType.HashSet;

		public void AddToCache<T>(RedisHashSetCachedObject<T> source, IDataContractResolver dataContractResolver = null)
		{
			if (!(source?.Id?.HasFullKey ?? false) || (source?.Value?.Equals(default(T)) ?? true))
			{
				return;
			}

			string compressedVal;

			ISerializationService sz =
				RedisService.GetSerializationService<RedisHashSetCachedObject<T>>(dataContractResolver);
			compressedVal = sz.SerializeObject<RedisHashSetCachedObject<T>>(source);

			HardAddToCache(source.Id, compressedVal);
		}

		public void DeleteFromCache(RedisId key)
		{
			// get all the keys to be deleted
			IEnumerable<RedisId> keys = GetKeys(key);
			Dictionary<string, List<string>> hashsetDictionary = new Dictionary<string, List<string>>();

			// sort the keys by hashset (there should only be 1, but this is more safe)
			foreach (RedisId currentKey in keys)
			{
				if (!hashsetDictionary.Keys.Contains(currentKey.HashSetIdentifier))
				{
					hashsetDictionary.Add(currentKey.HashSetIdentifier, new List<string>());
				}
				hashsetDictionary[currentKey.HashSetIdentifier].Add(currentKey.ObjectIdentifier);
			}

			// delete keys by hashset (still there should only be one)
			foreach (string hashsetDictionaryKey in hashsetDictionary.Keys)
			{
				RedisService.Database.HashDelete(hashsetDictionaryKey,
					hashsetDictionary[hashsetDictionaryKey].Select(x => (RedisValue)x).ToArray());
			}
		}

		public void DeleteFromCache(IEnumerable<RedisId> keys)
		{
			foreach (RedisId key in keys)
			{
				DeleteFromCache(key);
			}
		}

		public T GetFromCache<T>(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			T retVal = default(T);

			if (!(key?.HasFullKey ?? false))
			{
				return retVal;
			}

			string compressedVal = HardGetFromCache(key);
			// we didn't get a value
			if (string.IsNullOrWhiteSpace(compressedVal))
			{
				return retVal;
			}

			RedisHashSetCachedObject<T> wrapper =
				RedisService.ConvertString<RedisHashSetCachedObject<T>>(compressedVal, key, dataContractResolver);
			// the value isn't properly wrapped
			if (wrapper == null)
			{
				return retVal;
			}

			// we have a still valid key
			if (wrapper.ExpireTime >= DateTime.UtcNow)
			{
				return wrapper.Value;
			}

			// delete it if it wasn't valid
			DeleteFromCache(key);
			return retVal;
		}

		public IEnumerable<RedisId> GetKeys(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			if (!(key?.HasFullKey ?? false))
			{
				return new List<RedisId>();
			}

			IEnumerable<HashEntry> fieldIdentifiers = RedisService.Database.HashScan(key.HashSetIdentifier, key.ObjectIdentifier,
				RedisService.CacheSettings.DefaultScanPageSize);

			IEnumerable<RedisId> resultingKeys =
				// get the name, and add it to the hash name
				fieldIdentifiers.Select(fi => new RedisId { HashSetIdentifier = key.HashSetIdentifier, ObjectIdentifier = fi.Name.ToString(), ServiceType = this.ServiceType });
			return resultingKeys;
		}

		public IEnumerable<string> GetKeysStrings(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			return GetKeys(key).Select(k => k.FullKey);
		}

		public IEnumerable<T> GetListFromCache<T>(IList<RedisId> keys, IDataContractResolver dataContractResolver = null)
		{
			IEnumerable<Tuple<RedisId, string>> values = GetStringListFromCache<T>(keys, dataContractResolver);
			IEnumerable<RedisHashSetCachedObject<T>> wrappers = values
				.Select(value => RedisService.ConvertString<RedisHashSetCachedObject<T>>(value.Item2, value.Item1, dataContractResolver));

			// do some cache cleaning
			IEnumerable<RedisId> itemsToDelete = wrappers.Where(x => (x?.ExpireTime ?? DateTime.MinValue) < DateTime.UtcNow)
				.Select(x => x.Id);
			if (itemsToDelete.Any())
			{
				DeleteFromCache(itemsToDelete);
			}

			// only return valid values
			return wrappers.Where(x => (x?.ExpireTime ?? DateTime.MinValue) >= DateTime.UtcNow).Select(x => x.Value);
		}

		public IEnumerable<T> GetListFromCache<T>(RedisId key, IDataContractResolver dataContractResolver)
		{
			return GetListFromCache<T>(new[] { key }, dataContractResolver);
		}

		private IEnumerable<Tuple<RedisId, string>> GetStringListFromCache<T>(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			return GetStringListFromCache<T>(new[] { key }, dataContractResolver);
		}

		private IEnumerable<Tuple<RedisId, string>> GetStringListFromCache<T>(IList<RedisId> keys, IDataContractResolver dataContractResolver = null)
		{
			if (!(keys?.Any() ?? false))
			{
				return new List<Tuple<RedisId, string>>();
			}

			foreach (RedisId key in keys)
			{
				if (!(key?.HasFullKey ?? false))
				{
					continue;
				}
				key.ObjectIdentifier += "*";
			}
			List<Tuple<RedisId, string>> values = new List<Tuple<RedisId, string>>();

			foreach (RedisId key in keys)
			{
				if (!(key?.HasFullKey ?? false))
				{
					continue;
				}

				IEnumerable<HashEntry> fieldIdentifiers = RedisService.Database.HashScan(key.HashSetIdentifier, key.ObjectIdentifier,
					RedisService.CacheSettings.DefaultScanPageSize);
				IEnumerable<Tuple<RedisId, string>> resultingValues =
					// get the name, and add it to the hash name
					fieldIdentifiers
						.Select(fi => new Tuple<RedisId, string>(key, fi.Value.ToString()));
				values.AddRange(resultingValues);
			}

			return values;
		}

		public TimeSpan? GetTimeToLive<T>(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			if (!(key?.HasFullKey ?? false))
			{
				return null;
			}

			string compressedVal = HardGetFromCache(key);
			// we didn't get a value
			if (string.IsNullOrWhiteSpace(compressedVal))
			{
				return null;
			}

			RedisHashSetCachedObject<T> wrapper =
				RedisService.ConvertString<RedisHashSetCachedObject<T>>(compressedVal, key, dataContractResolver);
			// the value isn't properly wrapped
			if (wrapper == null)
			{
				return null;
			}

			// we have a still valid key
			if (wrapper.ExpireTime >= DateTime.UtcNow)
			{
				return wrapper.ExpireTime - DateTime.UtcNow;
			}

			// delete it if it wasn't valid
			DeleteFromCache(key);
			return TimeSpan.FromSeconds(0);
		}

		public Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IList<RedisId> keys, IDataContractResolver dataContractResolver = null)
		{
			IEnumerable<RedisHashSetCachedObject<T>> values = GetListFromCache<RedisHashSetCachedObject<T>>(keys, dataContractResolver);

			// do some cache cleaning
			IEnumerable<RedisId> itemsToDelete = values.Where(x => (x?.ExpireTime ?? DateTime.MinValue) < DateTime.UtcNow)
				.Select(x => x.Id);
			if (itemsToDelete.Any())
			{
				DeleteFromCache(itemsToDelete);
			}

			// only return valid values
			IEnumerable<RedisHashSetCachedObject<T>> goodWrappers =
				values.Where(x => (x?.ExpireTime ?? DateTime.MinValue) >= DateTime.UtcNow);
			Dictionary<RedisId, TimeSpan?> ttls = goodWrappers.ToDictionary(x => x.Id,
				hashSetWrapper => (TimeSpan?)(hashSetWrapper.ExpireTime - DateTime.UtcNow));
			return ttls;
		}

		protected virtual void HardAddToCache(RedisId key, string value)
		{
			RedisService.Database.HashSet(key.HashSetIdentifier, key.ObjectIdentifier, value);
		}

		protected virtual string HardGetFromCache(RedisId key)
		{
			return RedisService.Database.HashGet(key.HashSetIdentifier, key.ObjectIdentifier);
		}
	}
}