using System;
using System.Collections.Generic;
using System.Linq;
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
	internal class HashSetRedisService : ARedisService
	{
		public HashSetRedisService(RedisService redisService)
		{
			RedisService = redisService;
		}

		public override RedisService RedisService { get; }

		public override RedisServiceType ServiceType => RedisServiceType.HashSet;

		protected override void ServiceAdd(RedisId key, RedisValue value, DateTime expiration)
		{
			RedisService.Database.HashSet(key.HashSetIdentifier, key.ObjectIdentifier, RedisService.CacheProvider.CompressValue(value));
		}

		protected override RedisValue ServiceGet(RedisId key)
		{
			return RedisService.Database.HashGet(key.HashSetIdentifier, key.ObjectIdentifier);
		}

		public override bool ContainsKey(RedisId key)
		{
			HashEntry value = RedisService.Database.HashScan(key.HashSetIdentifier, key.ObjectIdentifier
				, RedisService.CacheSettings.ServiceSettings.DefaultScanPageSize).FirstOrDefault();
			return !string.IsNullOrWhiteSpace(value.Name);
		}

		public override long ContainsKeys(IEnumerable<RedisId> keys)
		{
			return keys.Select(ContainsKey).LongCount(x => x);
		}

		public override bool DeleteValue(RedisId key)
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

		public override long DeleteValues(IEnumerable<RedisId> keys)
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

		public override IEnumerable<RedisId> GetKey<T>(RedisId key)
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

		public override IEnumerable<RedisId> GetKeys<T>(IEnumerable<RedisId> keys)
		{
			List<RedisId> values = new List<RedisId>();
			foreach (RedisId key in keys)
			{
				values.AddRange(GetKey<T>(key));
			}
			return values;
		}

		public override Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IEnumerable<RedisId> keys)
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

		protected override IEnumerable<RedisValue> ServiceGetValues<T>(IEnumerable<RedisId> keys)
		{
			var expandedKeys = keys.Select(x => new 
				{
					HashSetIdentifier = x.HashSetIdentifier,
					Keys = GetKey<T>(x)
				}).ToArray();
			RedisValue[] results = expandedKeys.SelectMany(x =>
				x.Keys.Select(y =>
					RedisService.Database.HashGet(x.HashSetIdentifier, y.ObjectIdentifier)
				)).Where(x => x.HasValue).ToArray();
			
			return results;
		}

		protected override IEnumerable<RedisCachedObject<T>> ServiceValuePostProcess<T>(IEnumerable<RedisCachedObject<T>> results)
		{
			// do some cache cleaning
			IEnumerable<RedisId> itemsToDelete = results.Where(x => (x?.ExpireTime ?? DateTime.MinValue) < DateTime.UtcNow)
				.Select(x => x.Id);
			if (itemsToDelete.Any())
			{
				DeleteValues(itemsToDelete);
				string[] fullKeysDeleted = itemsToDelete.Where(x => x != null).Select(x => x.FullKey.Replace("*", "")).ToArray();
				// if it doesn't have a full key we assume it is deleted
				results = results.Where(x => !fullKeysDeleted.Any(y => x?.Id?.FullKey?.StartsWith(y) ?? true)).ToList();
			}

			return results;
		}

		// might be slow
		public override long KeyCount()
		{
			return RedisService.Database.HashScan(RedisService.CacheSettings.ServiceSettings.PrefixIdentifier
				, "*", this.RedisService.CacheSettings.ServiceSettings.DefaultScanPageSize).LongCount();
		}
	}
}