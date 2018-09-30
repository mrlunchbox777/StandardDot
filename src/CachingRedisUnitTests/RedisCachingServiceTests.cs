using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using StandardDot.Abstract.Caching;
using StandardDot.Caching.Redis.Dto;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.Caching.Redis.UnitTests
{
	public class RedisCachingServiceTests
	{
		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void BasicVerification(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			foreach (var thing in service)
			{
				Debug.Write("test");
			}
			Assert.Empty(service);
			Assert.Equal(cacheLifeTime, service.DefaultCacheLifespan);
			Assert.False(service.IsReadOnly);
			Assert.Empty(service.Keys);
			Assert.Empty(service.Values);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void Caching(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			Assert.Single(service);
			Assert.Equal(cacheLifeTime, service.DefaultCacheLifespan);
			Assert.False(service.IsReadOnly);
			Assert.Single(service.Keys);
			Assert.Equal(cachableKey, ((RedisId)service.Keys.Single()).ObjectIdentifier);
			Assert.Single(service.Values);
			string returnedString = service.Values.Single().UntypedValue as string;
			Assert.False(string.IsNullOrWhiteSpace(returnedString));
			Foobar returned = RedisHelpers.SerializationService.DeserializeObject<Foobar>(returnedString, RedisHelpers.SerializationSettings);
			Assert.NotNull(returned);
			Assert.NotEqual(cachable, returned);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, returned));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void Retrieval(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			DateTime startedCaching = DateTime.UtcNow;
			Thread.Sleep(10);
			service.Cache(cachableKey, cachable);
			Thread.Sleep(10);
			DateTime endedCaching = DateTime.UtcNow;

			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.NotNull(retrievedWrapper);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, retrievedWrapper.Value));
			Assert.True(retrievedWrapper.CachedTime >= startedCaching && retrievedWrapper.CachedTime <= endedCaching);
			Assert.True(retrievedWrapper.ExpireTime >= startedCaching.Add(cacheLifeTime)
				&& retrievedWrapper.ExpireTime <= endedCaching.Add(cacheLifeTime));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void ManualExpiration(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			DateTime startedCaching = DateTime.UtcNow;
			service.Cache(cachableKey, cachable);
			DateTime endedCaching = DateTime.UtcNow;
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);

			service.Invalidate(cachableKey);
			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.Empty(service);
			Assert.NotNull(existsRetrievedWrapper);
			Assert.Null(retrievedWrapper);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void AutomatedExpiration(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromSeconds(1);
			RedisCachingService service = RedisHelpers.GetCustomRedis(compressValues: compressValues, useBasic: useBasic, defaultExpireTimeSpanSeconds: (int)cacheLifeTime.TotalSeconds);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			DateTime startedCaching = DateTime.UtcNow;
			service.Cache(cachableKey, cachable);
			DateTime endedCaching = DateTime.UtcNow;
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);

			Thread.Sleep(1000);
			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.NotNull(existsRetrievedWrapper);
			Assert.Null(retrievedWrapper);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void StaticUsage(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromSeconds(1);
			RedisCachingService service = RedisHelpers.GetCustomRedis(compressValues: compressValues, useBasic: useBasic, defaultExpireTimeSpanSeconds: (int)cacheLifeTime.TotalSeconds);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			DateTime startedCaching = DateTime.UtcNow;
			service.Cache(cachableKey, cachable);
			DateTime endedCaching = DateTime.UtcNow;
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);

			RedisCachingService service2 = RedisHelpers.GetCustomRedis(compressValues: compressValues, useBasic: useBasic, defaultExpireTimeSpanSeconds: (int)cacheLifeTime.TotalSeconds);
			ICachedObject<Foobar> existsRetrievedWrapper2 = service2.Retrieve<Foobar>(cachableKey);

			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, existsRetrievedWrapper.Value));
			Assert.True(RedisHelpers.CheckFooBarEquality(existsRetrievedWrapper.Value, existsRetrievedWrapper2.Value));
			// they aren't reference equal
			Assert.NotEqual(existsRetrievedWrapper.Value, existsRetrievedWrapper2.Value);
			service2.Clear();
			Assert.Null(service.Retrieve<Foobar>(cachableKey));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void DoubleCache(bool compressValues, bool useBasic)
		{
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Foobar cachable2 = RedisHelpers.GetCachableObject();
			service.Cache(cachableKey, cachable2);

			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.NotNull(retrievedWrapper);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, existsRetrievedWrapper.Value));
			Assert.NotEqual(existsRetrievedWrapper.Value, retrievedWrapper.Value);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable2, retrievedWrapper.Value));
			Assert.True(RedisHelpers.CheckFooBarEquality(retrievedWrapper.Value, existsRetrievedWrapper.Value, false));
			Assert.Single(service);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void DoubleRetrieve(bool compressValues, bool useBasic)
		{
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.NotNull(existsRetrievedWrapper);
			Assert.NotNull(retrievedWrapper);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, existsRetrievedWrapper.Value));
			Assert.True(RedisHelpers.CheckFooBarEquality(existsRetrievedWrapper.Value, retrievedWrapper.Value));
			Assert.Single(service);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void AutomatedCustomExpiration(bool compressValues, bool useBasic)
		{
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			DateTime startedCaching = DateTime.UtcNow;
			// just setting it long enough that even slow machines can pass it
			service.Cache(cachableKey, cachable, DateTime.UtcNow, DateTime.UtcNow.AddMilliseconds(550)); ;
			DateTime endedCaching = DateTime.UtcNow;
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);

			Thread.Sleep(560);
			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.NotNull(existsRetrievedWrapper);
			Assert.Null(retrievedWrapper);
		}

		// Dictonary Tests

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void Keys(bool compressValues, bool useBasic)
		{
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Assert.Empty(service.Keys);
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			Assert.Single(service.Keys);
			Assert.Equal(cachableKey, ((RedisId)(service.Keys.Single())).ObjectIdentifier);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void Values(bool compressValues, bool useBasic)
		{
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Assert.Empty(service.Values);
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			Assert.Single(service.Values);
			Assert.NotNull(service.Values.Single());
			string returnedString = service.Values.Single().UntypedValue as string;
			Assert.False(string.IsNullOrWhiteSpace(returnedString));
			Foobar returned = RedisHelpers.SerializationService.DeserializeObject<Foobar>(returnedString, RedisHelpers.SerializationSettings);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, returned));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void Count(bool compressValues, bool useBasic)
		{
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Assert.Empty(service);
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			Assert.Single(service);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void Indexing(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			string cachableKey = RedisHelpers.CachableKey;
			// this shouldn't throw
			Assert.Null(service[cachableKey]);
			Foobar cachable = RedisHelpers.GetCachableObject();
			DefaultCachedObject<object> dto = new DefaultCachedObject<object>
			{
				Value = cachable,
				CachedTime = DateTime.UtcNow,
				ExpireTime = DateTime.UtcNow.Add(cacheLifeTime)
			};

			service[cachableKey] = dto;
			Assert.Single(service);
			ICachedObjectBasic retrieved = service[cachableKey];
			Foobar returned = RedisHelpers.SerializationService.DeserializeObject<Foobar>(
				(string)retrieved.UntypedValue, RedisHelpers.SerializationSettings);
			Assert.NotNull(returned);
			Assert.NotEqual(cachable, returned);
			Assert.True(RedisHelpers.CheckFooBarEquality((Foobar)dto.Value, returned));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void Add(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			string cachableKey = RedisHelpers.CachableKey;
			Foobar cachable = RedisHelpers.GetCachableObject();
			DefaultCachedObject<object> dto = new DefaultCachedObject<object>
			{
				Value = cachable,
				CachedTime = DateTime.UtcNow,
				ExpireTime = DateTime.UtcNow.Add(cacheLifeTime)
			};

			service.Add(cachableKey, dto);
			Assert.Single(service);
      var theSingle = service.Values.Single();
      var theValue = theSingle.UntypedValue;
			string returnedString = theValue as string;
			Assert.False(string.IsNullOrWhiteSpace(returnedString));
			Foobar returned = RedisHelpers.SerializationService.DeserializeObject<Foobar>(returnedString, RedisHelpers.SerializationSettings);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, returned));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void ContainsKey(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			string cachableKey = RedisHelpers.CachableKey;
			Foobar cachable = RedisHelpers.GetCachableObject();

			service.Cache<Foobar>(cachableKey, cachable);
			Assert.Single(service);
			Assert.True(service.ContainsKey(cachableKey));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void Remove(bool compressValues, bool useBasic)
		{
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			DateTime startedCaching = DateTime.UtcNow;
			service.Cache(cachableKey, cachable);
			DateTime endedCaching = DateTime.UtcNow;
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);

			service.Remove(cachableKey);
			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.Empty(service);
			Assert.NotNull(existsRetrievedWrapper);
			Assert.Null(retrievedWrapper);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void TryGetValue(bool compressValues, bool useBasic)
		{
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			string cachableKey = RedisHelpers.CachableKey;
			ICachedObjectBasic result;
			Assert.False(service.TryGetValue(cachableKey, out result));
			Foobar cachable = RedisHelpers.GetCachableObject();

			service.Cache(cachableKey, cachable);
			bool success = service.TryGetValue(cachableKey, out result);
			Assert.True(success);
			string returnedString = service.Values.Single().UntypedValue as string;
			Assert.False(string.IsNullOrWhiteSpace(returnedString));
			Foobar returned = RedisHelpers.SerializationService.DeserializeObject<Foobar>(returnedString, RedisHelpers.SerializationSettings);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, returned));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void AddKvp(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();
			string cachableKey = RedisHelpers.CachableKey;
			KeyValuePair<string, ICachedObjectBasic> item = RedisHelpers.GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);

			service.Add(item);
			ICachedObject<Foobar> result = service.Retrieve<Foobar>(cachableKey);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, result.Value));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void Clear(bool compressValues, bool useBasic)
		{
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			string cachableKey = RedisHelpers.CachableKey;
			Foobar cachable = RedisHelpers.GetCachableObject();

			service.Cache<Foobar>(cachableKey, cachable);
			Assert.Single(service);
			ICachedObject<Foobar> cached = service.Retrieve<Foobar>(cachableKey);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, cached.Value));

			service.Clear();
			Assert.Empty(service);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void Contains(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();
			string cachableKey = RedisHelpers.CachableKey;
			KeyValuePair<string, ICachedObjectBasic> item = RedisHelpers.GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);

			Assert.DoesNotContain(item, service);
			service.Add(item);
			Assert.Contains(item, service);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void ContainsBranching(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();
			string cachableKey = RedisHelpers.CachableKey;
			KeyValuePair<string, ICachedObjectBasic> originalItem = RedisHelpers.GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);
			KeyValuePair<string, ICachedObjectBasic> item;
			DateTime originalTime = originalItem.Value.CachedTime;

			Assert.DoesNotContain(originalItem, service);

			service.Clear();
			Assert.Empty(service);
			item = RedisHelpers.GetCachableKvp(originalTime, cacheLifeTime, null, cachableKey);
			service.Add(item);
			Assert.DoesNotContain(originalItem, service);

			service.Clear();
			Assert.Empty(service);
			item = RedisHelpers.GetCachableKvp(originalTime, TimeSpan.FromMinutes(-5), cachable, cachableKey);
			service.Add(item);
			Assert.DoesNotContain(originalItem, service);
			Assert.Empty(service);

			service.Clear();
			Assert.Empty(service);
			item = RedisHelpers.GetCachableKvp(originalTime.AddSeconds(10), cacheLifeTime, cachable, cachableKey);
			service.Add(item);
			Assert.DoesNotContain(originalItem, service);

			service.Clear();
			Assert.Empty(service);
			item = RedisHelpers.GetCachableKvp(originalTime, TimeSpan.FromMinutes(4), cachable, cachableKey);
			service.Add(item);
			Assert.DoesNotContain(originalItem, service);

			service.Clear();
			Assert.Empty(service);
			item = RedisHelpers.GetCachableKvp(originalTime, cacheLifeTime, RedisHelpers.GetCachableObject(), cachableKey);
			service.Add(item);
			Assert.DoesNotContain(originalItem, service);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void CopyTo(bool compressValues, bool useBasic)
		{
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			KeyValuePair<string, ICachedObjectBasic>[] cache = new KeyValuePair<string, ICachedObjectBasic>[1];
			service.CopyTo(cache, 0);
			Assert.NotNull(cache);
			Assert.NotEmpty(cache);
			Assert.Single(cache);
			Foobar returned = RedisHelpers.SerializationService.DeserializeObject<Foobar>((string)cache[0].Value.UntypedValue, RedisHelpers.SerializationSettings);
			Assert.NotNull(returned);
			Assert.NotEqual(cachable, returned);
			Assert.True(RedisHelpers.CheckFooBarEquality(existsRetrievedWrapper.Value, returned));
			Assert.Equal(cachableKey, ((RedisId)cache[0].Key).ObjectIdentifier);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void RemoveKvp(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();
			string cachableKey = RedisHelpers.CachableKey;
			KeyValuePair<string, ICachedObjectBasic> item = RedisHelpers.GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);
			bool succeded = service.Remove(item);
			Assert.False(succeded);

			service.Add(item);
			ICachedObject<Foobar> result = service.Retrieve<Foobar>(cachableKey);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, result.Value));
			succeded = service.Remove(item);
			Assert.True(succeded);
			Assert.Empty(service);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void RemoveBranching(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();
			string cachableKey = RedisHelpers.CachableKey;
			KeyValuePair<string, ICachedObjectBasic> originalItem = RedisHelpers.GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);
			KeyValuePair<string, ICachedObjectBasic> item;
			DateTime originalTime = originalItem.Value.CachedTime;

			Assert.False(service.Remove(originalItem)); ;

			service.Clear();
			Assert.Empty(service);
			item = RedisHelpers.GetCachableKvp(originalTime, cacheLifeTime, null, cachableKey);
			service.Add(item);
			Assert.False(service.Remove(originalItem));

			service.Clear();
			Assert.Empty(service);
			item = RedisHelpers.GetCachableKvp(originalTime, TimeSpan.FromMinutes(-5), cachable, cachableKey);
			service.Add(item);
			Assert.False(service.Remove(originalItem));
			Assert.Empty(service);

			service.Clear();
			Assert.Empty(service);
			item = RedisHelpers.GetCachableKvp(originalTime.AddSeconds(10), cacheLifeTime, cachable, cachableKey);
			service.Add(item);
			Assert.False(service.Remove(originalItem));

			service.Clear();
			Assert.Empty(service);
			item = RedisHelpers.GetCachableKvp(originalTime, TimeSpan.FromMinutes(4), cachable, cachableKey);
			service.Add(item);
			Assert.False(service.Remove(originalItem));

			service.Clear();
			Assert.Empty(service);
			item = RedisHelpers.GetCachableKvp(originalTime, cacheLifeTime, RedisHelpers.GetCachableObject(), cachableKey);
			service.Add(item);
			Assert.False(service.Remove(originalItem));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		public void TestEnumerators(bool compressValues, bool useBasic)
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis(compressValues: compressValues, useBasic: useBasic);
			service.Clear();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);

			IEnumerator<KeyValuePair<string, ICachedObjectBasic>> typedEnumerator
				= service.GetEnumerator();
			IEnumerator enumerator = ((IEnumerable)service).GetEnumerator();
			typedEnumerator.MoveNext();
			enumerator.MoveNext();

			Foobar returned = RedisHelpers.SerializationService.DeserializeObject<Foobar>(
				(string)((ICachedObjectBasic)(typedEnumerator.Current).Value).UntypedValue, RedisHelpers.SerializationSettings);
			Assert.NotNull(returned);
			Assert.NotEqual(cachable, returned);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, returned));

			Foobar returned2 = RedisHelpers.SerializationService.DeserializeObject<Foobar>(
				(string)((ICachedObjectBasic)((KeyValuePair<string, ICachedObjectBasic>)
					(enumerator.Current)).Value).UntypedValue, RedisHelpers.SerializationSettings);
			Assert.NotNull(returned2);
			Assert.NotEqual(cachable, returned2);
			Assert.True(RedisHelpers.CheckFooBarEquality(cachable, returned2));
		}
	}
}
