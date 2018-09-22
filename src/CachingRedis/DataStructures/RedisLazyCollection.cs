using System.Collections.Generic;
using System.Linq;
using StandardDot.Abstract.DataStructures;
using StandardDot.Caching.Redis.Abstract;

namespace StandardDot.Caching.Redis.DataStructures
{
	public class RedisLazyCollection<T, TK> : BaseLazyCollection<T>
		where T : IRedisCachedObject<TK>
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
			Service.Cache(item.Id, item);
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
			if (string.IsNullOrWhiteSpace(item?.Id?.ObjectIdentifier))
			{
				return false;
			}
			Source = Source.Where(x => x.Id.FullKey != item.Id.FullKey);
			return Service.Invalidate(item.Id);
		}
	}
}