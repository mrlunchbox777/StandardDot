using System;
using System.Collections.Generic;
using System.Linq;
using StandardDot.Abstract.DataStructures;
using StandardDot.Caching.Redis.Abstract;

namespace StandardDot.Caching.Redis.DataStructures
{
	public class RedisLazyCollection<T> : BaseLazyCollection<T>
	{
		public RedisLazyCollection(IEnumerable<T> source, RedisCachingService service)
			: base(source)
		{
			Service = service;
			Source = _source;
		}

		protected virtual IEnumerable<T> Source { get; set; }

		protected virtual RedisCachingService Service { get; }

		// slow and bad... is there a better way?
		public override int Count => Source.Count();

		public override bool IsReadOnly => false;

		public override void Add(T item)
		{
			IRedisCachedObject castedItem = GetCastedItem(item);
			Service.Cache(castedItem.Id, item);
		}

		public override void Clear()
		{
			while(Source?.Any() ?? false)
			{
				bool removed = Remove(Source.FirstOrDefault());
				if (!removed)
				{
					Source = Source.Skip(1);
				}
			}
		}

		public override bool Contains(T item)
		{
			return Source.Contains(item);
		}

		// slow and bad, but no better way
		public override void CopyTo(T[] array, int arrayIndex)
		{
			Source.ToArray().CopyTo(array, arrayIndex);
		}

		public override bool Remove(T item)
		{
			IRedisCachedObject castedItem = GetCastedItem(item, false);
			if (string.IsNullOrWhiteSpace(castedItem?.Id?.ObjectIdentifier))
			{
				return false;
			}
			Source = Source.Cast<IRedisCachedObject>().Where(x => x.Id.FullKey != castedItem.Id.FullKey).Cast<T>();
			return Service.Invalidate(castedItem.Id);
		}

		protected virtual IRedisCachedObject GetCastedItem(T item, bool throwException = true)
		{
			IRedisCachedObject castedItem = item as IRedisCachedObject;
			if (throwException && castedItem == null)
			{
				throw new InvalidOperationException("Cannot add an item that doesn't derive from IRedisCachedObject");
			}
			return castedItem;
		}
	}
}