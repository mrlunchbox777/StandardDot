using System;
using System.Collections;
using System.Collections.Generic;
using StandardDot.Abstract.Caching;

namespace StandardDot.Caching
{
    public class MemoryCachingService : ICachingService
    {
        public MemoryCachingService(TimeSpan defaultCacheLifespan, bool useStaticCache, IDictionary<string, ICachedObject<object>> cache = null)
        {
            DefaultCacheLifespan = defaultCacheLifespan;
            Store = useStaticCache ? _store : cache ?? new Dictionary<string, ICachedObject<object>>();
        }

        private static IDictionary<string, ICachedObject<object>> _store = new Dictionary<string, ICachedObject<object>>();

        protected virtual IDictionary<string, ICachedObject<object>> Store { get; }

        protected virtual ICachedObject<T> CreateCachedObject<T>(T value, DateTime cachedTime, DateTime expireTime)
        {
            return new DefaultCachedObject<T>
            {
                Value = value,
                CachedTime = cachedTime,
                ExpireTime = expireTime
            };
        }

        public virtual TimeSpan DefaultCacheLifespan { get; }

        public virtual ICollection<string> Keys => Store.Keys;

        public virtual ICollection<ICachedObject<object>> Values => Store.Values;

        public int Count => Store.Count;

        public bool IsReadOnly => Store.IsReadOnly;

        public ICachedObject<object> this[string key]
        {
            get => Retrieve<object>(key);
            set => Cache<object>(key, value, DateTime.UtcNow, DateTime.UtcNow.Add(DefaultCacheLifespan));
        }

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

        public void Cache<T>(string key, T value, DateTime cachedTime, DateTime expireTime)
        {
            Cache<T>(key, CreateCachedObject(value, cachedTime, expireTime));
        }

        public ICachedObject<T> Retrieve<T>(string key)
        {
            if (!ContainsKey(key))
            {
                return null;
            }

            ICachedObject<object> item = Store[key];
            if (!(item.Value is T))
            {
                return null;
            }
            T result = (T)item.Value;
            if (item.ExpireTime < DateTime.UtcNow)
            {
                Invalidate(key);
                return null;
            }
            return CreateCachedObject<T>(result, item.CachedTime, item.ExpireTime);
        }

        public bool Invalidate(string key)
        {
            if (ContainsKey(key))
            {
                return Store.Remove(key);
            }
            return false;
        }

        public void Add(string key, ICachedObject<object> value)
        {
            Cache<object>(key, value);
        }

        public bool ContainsKey(string key)
        {
            return Store.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return Invalidate(key);
        }

        public bool TryGetValue(string key, out ICachedObject<object> value)
        {
            value = Retrieve<object>(key);
            return value == null;
        }

        public void Add(KeyValuePair<string, ICachedObject<object>> item)
        {
            Cache(item.Key, item.Value);
        }

        public void Clear()
        {
            Store.Clear();
        }

        public bool Contains(KeyValuePair<string, ICachedObject<object>> item)
        {
            return Store.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, ICachedObject<object>>[] array, int arrayIndex)
        {
            Store.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, ICachedObject<object>> item)
        {
            return Store.Remove(item);
        }

        public IEnumerator<KeyValuePair<string, ICachedObject<object>>> GetEnumerator()
        {
            return Store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Store.GetEnumerator();
        }
    }
}
