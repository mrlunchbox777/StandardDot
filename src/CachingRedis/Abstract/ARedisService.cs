using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;
using StandardDot.Caching.Redis.Service;

namespace StandardDot.Caching.Redis.Abstract
{
	internal abstract class ARedisService : IRedisService
	{
		public abstract RedisServiceType ServiceType { get; }

		public abstract RedisService RedisService { get; }

		protected void HardAddToCache(RedisId key, RedisValue value, DateTime expiration)
		{
			RedisValue realValue = RedisService.CacheSettings.ServiceSettings.CompressValues
				? (RedisValue)RedisService.CacheProvider.CompressValue(value)
				: value;

			ServiceAdd(key, realValue, expiration);
		}

		protected RedisValue HardGetFromCache(RedisId key)
		{
			RedisValue rawValue = ServiceGet(key);
			RedisValue value = (rawValue != default(RedisValue)) && RedisService.CacheSettings.ServiceSettings.CompressValues
				? RedisService.CacheProvider.DecompressValue(rawValue)
				: rawValue;
			return value;
		}

		protected abstract void ServiceAdd(RedisId key, RedisValue value, DateTime expiration);

		protected abstract RedisValue ServiceGet(RedisId key);

		public abstract bool ContainsKey(RedisId key);

		public abstract long ContainsKeys(IEnumerable<RedisId> keys);

		public abstract bool DeleteValue(RedisId key);

		public abstract long DeleteValues(IEnumerable<RedisId> keys);

		public abstract IEnumerable<RedisId> GetKey<T>(RedisId key);

		public abstract IEnumerable<RedisId> GetKeys<T>(IEnumerable<RedisId> keys);

		public virtual Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(RedisId key)
		{
			return GetTimeToLive<T>(new[] { key });
		}

		public abstract Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IEnumerable<RedisId> keys);

		public virtual IEnumerable<RedisCachedObject<T>> GetValue<T>(RedisId key)
		{
			return GetValues<T>(new[] { key });
		}

		public IEnumerable<RedisCachedObject<T>> GetValues<T>(IEnumerable<RedisId> keys)
		{
			if (!(keys?.Any() ?? false))
			{
				return new RedisCachedObject<T>[0];
			}

			RedisId[] keysToUse = keys.ToArray();
			for (int i = 0; i < keysToUse.Count(); i++)
			{
				if (keysToUse[i].ObjectIdentifier.EndsWith("*"))
				{
					continue;
				}
				keysToUse[i].ObjectIdentifier = keysToUse[i].ObjectIdentifier + "*";
			}

			IEnumerable<RedisId> redisKeys = GetKeys<T>(keys);
			
			if (!(redisKeys?.Any() ?? false))
			{
				return new RedisCachedObject<T>[0];
			}

			RedisValue[] results = ServiceGetValues<T>(redisKeys).ToArray();
			List<RedisCachedObject<T>> values = new List<RedisCachedObject<T>>(results.Length);

			foreach (RedisValue result in results)
			{
				RedisCachedObject<T> current;
				RedisCachedObject<string> stringValue = RedisService.CacheProvider.GetCachedValue<string>(result, this);
				if (typeof(T) == typeof(object) || typeof(T) == typeof(string))
				{
					current = RedisService.CacheProvider.ChangeType<T, string>(stringValue);
				}
				else if (typeof(T).IsPrimitive)
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

			return ServiceValuePostProcess(values);
		}

		protected abstract IEnumerable<RedisValue> ServiceGetValues<T>(IEnumerable<RedisId> keys);

		protected abstract IEnumerable<RedisCachedObject<T>> ServiceValuePostProcess<T>(IEnumerable<RedisCachedObject<T>> results);

		public abstract long KeyCount();

		public virtual IEnumerable<RedisCachedObject<T>> SetValue<T>(RedisCachedObject<T> value)
		{
			if ((!(value?.Id?.HasFullKey ?? false)) || value.Value == null || value.Value.Equals(default(T)))
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
			HardAddToCache(value.Id, serializedWrapper, value.ExpireTime);
			return new[] { value };
		}

		public virtual IEnumerable<RedisCachedObject<T>> SetValues<T>(IEnumerable<RedisCachedObject<T>> values)
		{
			RedisCachedObject<T>[] valuesEnumerated = values.ToArray();
			for (int i = 0; i < valuesEnumerated.Length; i++)
			{
				valuesEnumerated[i] = SetValue(valuesEnumerated[i]).SingleOrDefault();
			}
			return valuesEnumerated;
		}
	}
}