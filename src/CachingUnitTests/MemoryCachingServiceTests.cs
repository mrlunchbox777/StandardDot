using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StandardDot.Abstract.Caching;
using Xunit;

namespace StandardDot.Caching.UnitTests
{
    public class MemoryCachingServiceTests
    {
        [Fact]
        public void BasicVerification()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
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
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            service.Cache(cachableKey, cachable);
            Assert.Single(service);
            Assert.Equal(cacheLifeTime, service.DefaultCacheLifespan);
            Assert.False(service.IsReadOnly);
            Assert.Single(service.Keys);
            Assert.Equal(cachableKey, service.Keys.Single());
            Assert.Single(service.Values);
            Assert.Equal(cachable, service.Values.Single().Value);
        }

        [Fact]
        public void Retrieval()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            DateTime startedCaching = DateTime.UtcNow;
            service.Cache(cachableKey, cachable);
            DateTime endedCaching = DateTime.UtcNow;

            ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
            Assert.NotNull(retrievedWrapper);
            Assert.Equal(cachable, retrievedWrapper.Value);
            Assert.True(retrievedWrapper.CachedTime > startedCaching && retrievedWrapper.CachedTime < endedCaching);
            Assert.True(retrievedWrapper.ExpireTime > startedCaching.Add(cacheLifeTime)
                && retrievedWrapper.ExpireTime < endedCaching.Add(cacheLifeTime));
        }

        [Fact]
        public void ManualExpiration()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
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
            TimeSpan cacheLifeTime = TimeSpan.FromMilliseconds(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            DateTime startedCaching = DateTime.UtcNow;
            service.Cache(cachableKey, cachable);
            DateTime endedCaching = DateTime.UtcNow;
            ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);

            Thread.Sleep(6);
            ICachedObject<Foobar> retrievedWrapper = service.Retrieve<Foobar>(cachableKey);
            Assert.NotNull(existsRetrievedWrapper);
            Assert.Null(retrievedWrapper);
        }

        [Fact]
        public void StaticUsage()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMilliseconds(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime, true);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            DateTime startedCaching = DateTime.UtcNow;
            service.Cache(cachableKey, cachable);
            DateTime endedCaching = DateTime.UtcNow;
            ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);

            MemoryCachingService service2 = new MemoryCachingService(cacheLifeTime, true);
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
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            service.Cache(cachableKey, cachable);
            ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);
            Foobar cachable2 = GetCachableObject();
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
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
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
        public void InjectedCache()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            service.Cache(cachableKey, cachable);
            ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);
            KeyValuePair<string, ICachedObject<object>>[] cache = new KeyValuePair<string, ICachedObject<object>>[1];
            service.CopyTo(cache, 0);
            Assert.NotNull(cache);
            Assert.NotEmpty(cache);
            Assert.Single(cache);
            Assert.Equal(existsRetrievedWrapper.Value, cache[0].Value.Value);
            Assert.Equal(cachableKey, cache[0].Key);

            MemoryCachingService service2 = new MemoryCachingService(cacheLifeTime, cache.ToDictionary(x => x.Key, y => y.Value));
            ICachedObject<Foobar> existsRetrievedWrapper2 = service2.Retrieve<Foobar>(cachableKey);
            Assert.Equal(existsRetrievedWrapper.Value, existsRetrievedWrapper2.Value);
        }

        [Fact]
        public void AutomatedCustomExpiration()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            DateTime startedCaching = DateTime.UtcNow;
            service.Cache(cachableKey, cachable, DateTime.UtcNow, DateTime.UtcNow.AddMilliseconds(5));;
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
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Assert.Empty(service.Keys);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            service.Cache(cachableKey, cachable);
            Assert.Single(service.Keys);
            Assert.Equal(cachableKey, service.Keys.Single());
        }

        [Fact]
        public void Values()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Assert.Empty(service.Values);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            service.Cache(cachableKey, cachable);
            Assert.Single(service.Values);
            Assert.NotNull(service.Values.Single());
            Assert.Equal(cachable, service.Values.Single().Value);
        }

        [Fact]
        public void Count()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Assert.Empty(service);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            service.Cache(cachableKey, cachable);
            Assert.Single(service);
        }

        [Fact]
        public void Indexing()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            string cachableKey = this.cachableKey;
            // this shouldn't throw
            Assert.Null(service[cachableKey]);
            Foobar cachable = GetCachableObject();
            DefaultCachedObject<object> dto = new DefaultCachedObject<object>
            {
                Value = cachable,
                CachedTime = DateTime.UtcNow,
                ExpireTime = DateTime.UtcNow.Add(cacheLifeTime)
            };

            service[cachableKey] = dto;
            Assert.Single(service);
            ICachedObject<object> retrieved = service[cachableKey];
            Assert.Equal(dto.Value, retrieved.Value);
        }

        [Fact]
        public void Add()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            string cachableKey = this.cachableKey;
            Foobar cachable = GetCachableObject();
            DefaultCachedObject<object> dto = new DefaultCachedObject<object>
            {
                Value = cachable,
                CachedTime = DateTime.UtcNow,
                ExpireTime = DateTime.UtcNow.Add(cacheLifeTime)
            };

            service.Add(cachableKey, dto);
            Assert.Single(service);;
            Assert.Equal(dto.Value, service.Retrieve<Foobar>(cachableKey).Value);
        }

        [Fact]
        public void ContainsKey()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            string cachableKey = this.cachableKey;
            Foobar cachable = GetCachableObject();

            service.Cache<Foobar>(cachableKey, cachable);
            Assert.Single(service);;
            Assert.True(service.ContainsKey(cachableKey));
        }

        [Fact]
        public void Remove()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
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
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            string cachableKey = this.cachableKey;
            ICachedObject<object> result;
            Assert.False(service.TryGetValue(cachableKey, out result));
            Foobar cachable = GetCachableObject();

            service.Cache(cachableKey, cachable);
            bool success = service.TryGetValue(cachableKey, out result);
            Assert.True(success);
            Assert.Equal(cachable, result.Value);
        }

        [Fact]
        public void AddKvp()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();
            string cachableKey = this.cachableKey;
            KeyValuePair<string, ICachedObject<object>> item = GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);

            service.Add(item);
            ICachedObject<Foobar> result = service.Retrieve<Foobar>(cachableKey);
            Assert.Equal(cachable, result.Value);
        }

        [Fact]
        public void Clear()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            string cachableKey = this.cachableKey;
            Foobar cachable = GetCachableObject();

            service.Cache<Foobar>(cachableKey, cachable);
            Assert.Single(service);;
            Assert.Equal(cachable, service.Retrieve<Foobar>(cachableKey).Value);

            service.Clear();
            Assert.Empty(service);
        }

        [Fact]
        public void Contains()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();
            string cachableKey = this.cachableKey;
            KeyValuePair<string, ICachedObject<object>> item = GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);

            Assert.DoesNotContain(item, service);
            service.Add(item);
            Assert.Contains(item, service);
        }

        [Fact]
        public void ContainsBranching()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();
            string cachableKey = this.cachableKey;
            KeyValuePair<string, ICachedObject<object>> originalItem = GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);
            KeyValuePair<string, ICachedObject<object>> item;
            DateTime originalTime = originalItem.Value.CachedTime;

            Assert.DoesNotContain(originalItem, service);

            service.Clear();
            Assert.Empty(service);
            item = GetCachableKvp(originalTime, cacheLifeTime, null, cachableKey);
            service.Add(item);
            Assert.DoesNotContain(originalItem, service);

            service.Clear();
            Assert.Empty(service);
            item = GetCachableKvp(originalTime, TimeSpan.FromMinutes(-5), cachable, cachableKey);
            service.Add(item);
            Assert.DoesNotContain(originalItem, service);
            Assert.Empty(service);

            service.Clear();
            Assert.Empty(service);
            item = GetCachableKvp(originalTime.AddMilliseconds(10), cacheLifeTime, cachable, cachableKey);
            service.Add(item);
            Assert.DoesNotContain(originalItem, service);

            service.Clear();
            Assert.Empty(service);
            item = GetCachableKvp(originalTime, TimeSpan.FromMinutes(4), cachable, cachableKey);
            service.Add(item);
            Assert.DoesNotContain(originalItem, service);

            service.Clear();
            Assert.Empty(service);
            item = GetCachableKvp(originalTime, cacheLifeTime, GetCachableObject(), cachableKey);
            service.Add(item);
            Assert.DoesNotContain(originalItem, service);
        }

        [Fact]
        public void CopyTo()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            service.Cache(cachableKey, cachable);
            ICachedObject<Foobar> existsRetrievedWrapper = service.Retrieve<Foobar>(cachableKey);
            KeyValuePair<string, ICachedObject<object>>[] cache = new KeyValuePair<string, ICachedObject<object>>[1];
            service.CopyTo(cache, 0);
            Assert.NotNull(cache);
            Assert.NotEmpty(cache);
            Assert.Single(cache);
            Assert.Equal(existsRetrievedWrapper.Value, cache[0].Value.Value);
            Assert.Equal(cachableKey, cache[0].Key);
        }

        [Fact]
        public void RemoveKvp()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();
            string cachableKey = this.cachableKey;
            KeyValuePair<string, ICachedObject<object>> item = GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);
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
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();
            string cachableKey = this.cachableKey;
            KeyValuePair<string, ICachedObject<object>> originalItem = GetCachableKvp(DateTime.UtcNow, cacheLifeTime, cachable, cachableKey);
            KeyValuePair<string, ICachedObject<object>> item;
            DateTime originalTime = originalItem.Value.CachedTime;

            Assert.False(service.Remove(originalItem));;

            service.Clear();
            Assert.Empty(service);
            item = GetCachableKvp(originalTime, cacheLifeTime, null, cachableKey);
            service.Add(item);
            Assert.False(service.Remove(originalItem));

            service.Clear();
            Assert.Empty(service);
            item = GetCachableKvp(originalTime, TimeSpan.FromMinutes(-5), cachable, cachableKey);
            service.Add(item);
            Assert.False(service.Remove(originalItem));
            Assert.Empty(service);

            service.Clear();
            Assert.Empty(service);
            item = GetCachableKvp(originalTime.AddMilliseconds(10), cacheLifeTime, cachable, cachableKey);
            service.Add(item);
            Assert.False(service.Remove(originalItem));

            service.Clear();
            Assert.Empty(service);
            item = GetCachableKvp(originalTime, TimeSpan.FromMinutes(4), cachable, cachableKey);
            service.Add(item);
            Assert.False(service.Remove(originalItem));

            service.Clear();
            Assert.Empty(service);
            item = GetCachableKvp(originalTime, cacheLifeTime, GetCachableObject(), cachableKey);
            service.Add(item);
            Assert.False(service.Remove(originalItem));
        }

        [Fact]
        public void TestEnumerators()
        {
            TimeSpan cacheLifeTime = TimeSpan.FromMinutes(5);
            MemoryCachingService service = new MemoryCachingService(cacheLifeTime);
            Foobar cachable = GetCachableObject();

            string cachableKey = this.cachableKey;
            service.Cache(cachableKey, cachable);

            IEnumerator<KeyValuePair<string, ICachedObject<object>>> typedEnumerator
                = service.GetEnumerator();
            IEnumerator enumerator = ((IEnumerable)service).GetEnumerator();
            typedEnumerator.MoveNext();
            enumerator.MoveNext();
            Assert.Equal(cachable,
                (Foobar)((ICachedObject<object>)(typedEnumerator.Current).Value).Value);
            Assert.Equal(cachable,
                (Foobar)((ICachedObject<object>)((KeyValuePair<string, ICachedObject<object>>)
                    (enumerator.Current)).Value).Value);
        }

        private Random _random = new Random();

        private Foobar GetCachableObject()
        {
            Foobar cachable = new Foobar
            {
                Foo = _random.Next(-1000, 1000),
                Bar = _random.Next(-1000, 1000)
            };
            return cachable;
        }

        private string cachableKey =>  _random.Next(100, 100).ToString();

        private KeyValuePair<string, ICachedObject<object>> GetCachableKvp(DateTime originalTime,
            TimeSpan cacheLifeTime, Foobar cachable, string cachableKey)
        {
            return new KeyValuePair<string, ICachedObject<object>>
            (
                cachableKey,
                new DefaultCachedObject<object>
                {
                    Value = cachable,
                    ExpireTime = originalTime.Add(cacheLifeTime),
                    CachedTime = originalTime
                }
            );
        }
    }
}
