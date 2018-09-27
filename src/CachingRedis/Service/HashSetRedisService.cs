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
using StandardDot.Abstract.DataStructures;

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

		protected virtual void HardAddToCache(RedisId key, string value)
		{
			if (RedisService.CacheSettings.ServiceSettings.CompressValues)
			{
				RedisService.Database.HashSet(key.HashSetIdentifier, key.ObjectIdentifier, RedisService.CacheProvider.CompressValue(value));
			}
			else
			{
				RedisService.Database.HashSet(key.HashSetIdentifier, key.ObjectIdentifier, value);
			}
		}

		protected virtual string HardGetFromCache(RedisId key)
		{
			RedisValue rawValue = RedisService.Database.HashGet(key.HashSetIdentifier, key.ObjectIdentifier);
			RedisValue value = (default(RedisValue) != rawValue) && RedisService.CacheSettings.ServiceSettings.CompressValues
				? RedisService.CacheProvider.DecompressValue(rawValue)
				: (string)rawValue;
			return value;
		}

		// Implementation of Abstract

		public IEnumerable<RedisId> GetKeys<T>(IEnumerable<RedisId> keys)
		{
			List<RedisId> values = new List<RedisId>();
			foreach (RedisId key in keys)
			{
				values.AddRange(GetKey<T>(key));
			}
			return values;
		}

		public IEnumerable<RedisId> GetKey<T>(RedisId key)
		{
			if (!(key?.HasFullKey ?? false))
			{
				return new List<RedisId>();
			}

			IEnumerable<HashEntry> fieldIdentifiers = RedisService.Database.HashScan(key.HashSetIdentifier, key.ObjectIdentifier,
				RedisService.CacheSettings.ServiceSettings.DefaultScanPageSize);

			IEnumerable<RedisId> resultingKeys =
				// get the name, and add it to the hash name
				fieldIdentifiers.Select(fi => new RedisId { HashSetIdentifier = key.HashSetIdentifier, ObjectIdentifier = fi.Name.ToString(), ServiceType = this.ServiceType });
			return resultingKeys;
		}

		public IEnumerable<RedisCachedObject<T>> GetValues<T>(IEnumerable<RedisId> keys)
		{
			if (!(keys?.Any() ?? false))
			{
				return new[] { RedisService.CacheProvider.CreateCachedValue<T>() };
			}

			RedisId[] keysToUse = keys.ToArray();
			for (int i = 0; i < keysToUse.Count(); i++)
			{
				keysToUse[i].ObjectIdentifier = keysToUse[i].ObjectIdentifier + "*";
			}

			// get the keys, in a non-blocking way
			var redisKeys = keysToUse.Select(x => new
			{
				HashSetIdentifier = (RedisKey)x.HashSetIdentifier,
				Keys = GetKey<T>(x)
			});

			if (!redisKeys.Any())
			{
				return new[] { RedisService.CacheProvider.CreateCachedValue<T>() };
			}

			string[] results = redisKeys.SelectMany(x =>
				x.Keys.Select(y =>
					RedisService.Database.HashGet(x.HashSetIdentifier, y.ObjectIdentifier)
					.ToString()
				)).ToArray();
			List<RedisCachedObject<T>> values = new List<RedisCachedObject<T>>(results.Length);
			foreach (RedisValue result in results)
			{
				RedisCachedObject<T> current;
				RedisCachedObject<string> stringValue = RedisService.CacheProvider.GetCachedValue<string>(result, this);
				if (typeof(T) == typeof(object))
				{
					current = RedisService.CacheProvider.ChangeType<T, string>(stringValue);
				}
				else
				{
					ISerializationService sz =
						RedisService.GetSerializationService<T>();
					current =  new RedisCachedObject<T>
					{
						RetrievedSuccesfully = stringValue.RetrievedSuccesfully,
						Value = sz.DeserializeObject<T>(stringValue.Value, RedisService.CacheSettings.SerializationSettings),
						CachedTime = stringValue.CachedTime,
						Status = stringValue.Status,
						Metadata = stringValue.Metadata,
						Id = stringValue.Id,
						ExpireTime = stringValue.ExpireTime,
					};
				}
				if (current != null)
				{
					values.Add(current);
				}
			}

			// do some cache cleaning
			IEnumerable<RedisId> itemsToDelete = values.Where(x => (x?.ExpireTime ?? DateTime.MinValue) < DateTime.UtcNow)
				.Select(x => x.Id);
			if (itemsToDelete.Any())
			{
				DeleteValues(itemsToDelete);
				string[] fullKeysDeleted = itemsToDelete.Select(x => x.FullKey.Replace("*", "")).ToArray();
				// if it doesn't have a full key we assume it is deleted
				values = values.Where(x => !fullKeysDeleted.Any(y => x?.Id?.FullKey?.StartsWith(y) ?? true)).ToList();
			}

			return values;
		}

		public IEnumerable<RedisCachedObject<T>> GetValue<T>(RedisId key)
		{
			return GetValues<T>(new[] { key });
		}

		public IEnumerable<RedisCachedObject<T>> SetValues<T>(IEnumerable<RedisCachedObject<T>> values)
		{
			RedisCachedObject<T>[] valuesEnumerated = values.ToArray();
			for (int i = 0; i < valuesEnumerated.Length; i++)
			{
				valuesEnumerated[i] = SetValue(valuesEnumerated[i]).SingleOrDefault();
			}
			return valuesEnumerated;
		}

		public IEnumerable<RedisCachedObject<T>> SetValue<T>(RedisCachedObject<T> value)
		{
			if (!(value?.Id?.HasFullKey ?? false) || (value?.Value?.Equals(default(T)) ?? true))
			{
				return null;
			}

			string serializedWrapper;
			string serializedObject;
			ISerializationService sz =
				RedisService.GetSerializationService<T>();
			serializedObject = sz.SerializeObject<T>(value.Value, RedisService.CacheSettings.SerializationSettings);

			RedisCachedObject<string> wrapper = new RedisCachedObject<string>
			{
				RetrievedSuccesfully = value.RetrievedSuccesfully,
				Value = serializedObject,
				CachedTime = value.CachedTime,
				Status = value.Status,
				Metadata = value.Metadata,
				Id = value.Id,
				ExpireTime = value.ExpireTime,
			};
			sz = RedisService.GetSerializationService<RedisCachedObject<string>>();
			serializedWrapper = sz.SerializeObject<RedisCachedObject<string>>(wrapper, RedisService.CacheSettings.SerializationSettings);

			HardAddToCache(value.Id, serializedWrapper);

			return new[] { value };
		}

		public long DeleteValues(IEnumerable<RedisId> keys)
		{
			long deleteCount = 0;
			foreach (RedisId key in keys)
			{
				if (DeleteValue(key))
				{
					deleteCount++;
				}
			}
			return deleteCount;
		}

		public bool DeleteValue(RedisId key)
		{
			// get all the keys to be deleted
			IEnumerable<RedisId> keys = GetKey<object>(key);
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

			long deleteCount = 0;
			// delete keys by hashset (still there should only be one)
			foreach (string hashsetDictionaryKey in hashsetDictionary.Keys)
			{
				deleteCount += RedisService.Database.HashDelete(hashsetDictionaryKey,
					hashsetDictionary[hashsetDictionaryKey].Select(x => (RedisValue)x).ToArray());
			}

			return deleteCount > 0;
		}

		// might be slow
		public long KeyCount()
		{
			return RedisService.Database.HashScan(RedisService.CacheSettings.ServiceSettings.PrefixIdentifier
				, "*", this.RedisService.CacheSettings.ServiceSettings.DefaultScanPageSize).LongCount();
		}

		public Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(RedisId key)
		{
			return GetTimeToLive<T>(new[] { key });
		}

		public Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IEnumerable<RedisId> keys)
		{
			IEnumerable<RedisCachedObject<T>> values = GetValues<T>(keys);

			// do some cache cleaning
			IEnumerable<RedisId> itemsToDelete = values.Where(x => (x?.ExpireTime ?? DateTime.MinValue) < DateTime.UtcNow)
				.Select(x => x.Id);
			if (itemsToDelete.Any())
			{
				DeleteValues(itemsToDelete);
			}

			// only return valid values
			IEnumerable<RedisCachedObject<T>> goodWrappers =
				values.Where(x => (x?.ExpireTime ?? DateTime.MinValue) >= DateTime.UtcNow);
			Dictionary<RedisId, TimeSpan?> ttls = goodWrappers.ToDictionary(x => x.Id,
				hashSetWrapper => (TimeSpan?)(hashSetWrapper.ExpireTime - DateTime.UtcNow));
			return ttls;
		}

		public long ContainsKeys(IEnumerable<RedisId> keys)
		{
			return keys.Select(ContainsKey).LongCount(x => x);
		}

		public bool ContainsKey(RedisId key)
		{
			return string.IsNullOrWhiteSpace((RedisService.Database.HashScan(key.HashSetIdentifier, key.ObjectIdentifier
				, this.RedisService.CacheSettings.ServiceSettings.DefaultScanPageSize).FirstOrDefault()).Name);
		}
	}
}