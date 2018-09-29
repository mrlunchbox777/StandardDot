using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.Configuration;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Dto;
using StandardDot.CoreServices.Serialization;
using StandardDot.TestClasses;
using Xunit;

namespace StandardDot.Caching.Redis.UnitTests
{
	public class RedisCachingServiceTests
	{
		[Fact]
		public void BasicVerification()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
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

		[Fact]
		public void Caching()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
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
			Assert.Equal(cachable.Foo, returned.Foo);
			Assert.NotEqual(cachable.Bar, returned.Bar);
			Assert.Equal(default(int), returned.Bar);
		}

		[Fact]
		public void Retrieval()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			DateTime startedCaching = DateTime.UtcNow;
			service.Cache(cachableKey, cachable);
			DateTime endedCaching = DateTime.UtcNow;

			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.NotNull(retrievedWrapper);
			Assert.Equal(cachable.Foo, retrievedWrapper.Value.Foo);
			Assert.NotEqual(cachable.Bar, retrievedWrapper.Value.Bar);
			Assert.Equal(default(int), retrievedWrapper.Value.Bar);
			Assert.True(retrievedWrapper.CachedTime > startedCaching && retrievedWrapper.CachedTime < endedCaching);
			Assert.True(retrievedWrapper.ExpireTime > startedCaching.Add(cacheLifeTime)
				&& retrievedWrapper.ExpireTime < endedCaching.Add(cacheLifeTime));
		}

		[Fact]
		public void ManualExpiration()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
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

		[Fact]
		public void AutomatedExpiration()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromSeconds(1);
			RedisCachingService service = RedisHelpers.GetCustomRedis(defaultExpireTimeSpanSeconds: (int)cacheLifeTime.TotalSeconds);
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

		[Fact]
		public void StaticUsage()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMilliseconds(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			DateTime startedCaching = DateTime.UtcNow;
			service.Cache(cachableKey, cachable);
			DateTime endedCaching = DateTime.UtcNow;
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);

			RedisCachingService service2 = RedisHelpers.GetRedis();
			ICachedObject<Foobar> existsRetrievedWrapper2 = service2.Retrieve<Foobar>(cachableKey);

			Assert.Equal(cachable, existsRetrievedWrapper.Value);
			Assert.Equal(existsRetrievedWrapper.Value, existsRetrievedWrapper2.Value);
			service2.Clear();
			Assert.Null(service.Retrieve<Foobar>(cachableKey));
		}

		[Fact]
		public void DoubleCache()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Foobar cachable2 = RedisHelpers.GetCachableObject();
			service.Cache(cachableKey, cachable2);

			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.NotNull(retrievedWrapper);
			Assert.Equal(cachable2, retrievedWrapper.Value);
			Assert.NotEqual(existsRetrievedWrapper.Value, retrievedWrapper.Value);
			Assert.Single(service);
		}

		[Fact]
		public void DoubleRetrieve()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.NotNull(existsRetrievedWrapper);
			Assert.NotNull(retrievedWrapper);
			Assert.Equal(cachable, existsRetrievedWrapper.Value);
			Assert.Equal(retrievedWrapper.Value, existsRetrievedWrapper.Value);
			Assert.Single(service);
		}

		[Fact]
		public void AutomatedCustomExpiration()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			DateTime startedCaching = DateTime.UtcNow;
			service.Cache(cachableKey, cachable, DateTime.UtcNow, DateTime.UtcNow.AddMilliseconds(5)); ;
			DateTime endedCaching = DateTime.UtcNow;
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);

			Thread.Sleep(6);
			ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			Assert.NotNull(existsRetrievedWrapper);
			Assert.Null(retrievedWrapper);
		}

		// Dictonary Tests

		[Fact]
		public void Keys()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Assert.Empty(service.Keys);
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			Assert.Single(service.Keys);
			Assert.Equal(cachableKey, service.Keys.Single());
		}

		[Fact]
		public void Values()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Assert.Empty(service.Values);
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			Assert.Single(service.Values);
			Assert.NotNull(service.Values.Single());
			Assert.Equal(cachable, service.Values.Single().UntypedValue);
		}

		[Fact]
		public void Count()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Assert.Empty(service);
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			Assert.Single(service);
		}

		[Fact]
		public void Indexing()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
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
			Assert.Equal(dto.Value, retrieved.UntypedValue);
		}

		[Fact]
		public void Add()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			string cachableKey = RedisHelpers.CachableKey;
			Foobar cachable = RedisHelpers.GetCachableObject();
			DefaultCachedObject<object> dto = new DefaultCachedObject<object>
			{
				Value = cachable,
				CachedTime = DateTime.UtcNow,
				ExpireTime = DateTime.UtcNow.Add(cacheLifeTime)
			};

			service.Add(cachableKey, dto);
			Assert.Single(service); ;
			Assert.Equal(dto.Value, service.Retrieve<Foobar>(cachableKey).Value);
		}

		[Fact]
		public void ContainsKey()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			string cachableKey = RedisHelpers.CachableKey;
			Foobar cachable = RedisHelpers.GetCachableObject();

			service.Cache<Foobar>(cachableKey, cachable);
			Assert.Single(service); ;
			Assert.True(service.ContainsKey(cachableKey));
		}

		[Fact]
		public void Remove()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
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

		[Fact]
		public void TryGetValue()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			string cachableKey = RedisHelpers.CachableKey;
			ICachedObjectBasic result;
			Assert.False(service.TryGetValue(cachableKey, out result));
			Foobar cachable = RedisHelpers.GetCachableObject();

			service.Cache(cachableKey, cachable);
			bool success = service.TryGetValue(cachableKey, out result);
			Assert.True(success);
			Assert.Equal(cachable, result.UntypedValue);
		}

		[Fact]
		public void AddKvp()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Foobar cachable = RedisHelpers.GetCachableObject();
			string cachableKey = RedisHelpers.CachableKey;
			KeyValuePair<string, ICachedObjectBasic> item = RedisHelpers.GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);

			service.Add(item);
			ICachedObject<Foobar> result = service.Retrieve<Foobar>(cachableKey);
			Assert.Equal(cachable, result.Value);
		}

		[Fact]
		public void Clear()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			string cachableKey = RedisHelpers.CachableKey;
			Foobar cachable = RedisHelpers.GetCachableObject();

			service.Cache<Foobar>(cachableKey, cachable);
      Assert.Single(service);
      ICachedObject<Foobar> cached = service.Retrieve<Foobar>(cachableKey);
      Assert.Equal(cachable, cached.Value);

			service.Clear();
			Assert.Empty(service);
		}

		[Fact]
		public void Contains()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Foobar cachable = RedisHelpers.GetCachableObject();
			string cachableKey = RedisHelpers.CachableKey;
			KeyValuePair<string, ICachedObjectBasic> item = RedisHelpers.GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);

			Assert.DoesNotContain(item, service);
			service.Add(item);
			Assert.Contains(item, service);
		}

		[Fact]
		public void ContainsBranching()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
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
			item = RedisHelpers.GetCachableKvp(originalTime.AddMilliseconds(10), cacheLifeTime, cachable, cachableKey);
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

		[Fact]
		public void CopyTo()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);
			ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);
			KeyValuePair<string, ICachedObjectBasic>[] cache = new KeyValuePair<string, ICachedObjectBasic>[1];
			service.CopyTo(cache, 0);
			Assert.NotNull(cache);
			Assert.NotEmpty(cache);
			Assert.Single(cache);
			Assert.Equal(existsRetrievedWrapper.Value, cache[0].Value.UntypedValue);
			Assert.Equal(cachableKey, cache[0].Key);
		}

		[Fact]
		public void RemoveKvp()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Foobar cachable = RedisHelpers.GetCachableObject();
			string cachableKey = RedisHelpers.CachableKey;
			KeyValuePair<string, ICachedObjectBasic> item = RedisHelpers.GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);
			bool succeded = service.Remove(item);
			Assert.False(succeded);

			service.Add(item);
			ICachedObject<Foobar> result = service.Retrieve<Foobar>(cachableKey);
			Assert.Equal(cachable, result.Value);
			succeded = service.Remove(item);
			Assert.True(succeded);
			Assert.Empty(service);
		}

		[Fact]
		public void RemoveBranching()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
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
			item = RedisHelpers.GetCachableKvp(originalTime.AddMilliseconds(10), cacheLifeTime, cachable, cachableKey);
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

		[Fact]
		public void TestEnumerators()
		{
			TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
			RedisCachingService service = RedisHelpers.GetRedis();
			Foobar cachable = RedisHelpers.GetCachableObject();

			string cachableKey = RedisHelpers.CachableKey;
			service.Cache(cachableKey, cachable);

			IEnumerator<KeyValuePair<string, ICachedObjectBasic>> typedEnumerator
				= service.GetEnumerator();
			IEnumerator enumerator = ((IEnumerable)service).GetEnumerator();
			typedEnumerator.MoveNext();
			enumerator.MoveNext();
			Assert.Equal(cachable,
				(Foobar)((ICachedObjectBasic)(typedEnumerator.Current).Value).UntypedValue);
			Assert.Equal(cachable,
				(Foobar)((ICachedObjectBasic)((KeyValuePair<string, ICachedObjectBasic>)
					(enumerator.Current)).Value).UntypedValue);
		}
	}
}
