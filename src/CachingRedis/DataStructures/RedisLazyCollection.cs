using System;
using System.Collections.Generic;
using System.Linq;
using StandardDot.Abstract.DataStructures;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.CoreExtensions;

namespace StandardDot.Caching.Redis.DataStructures
{
	/// <summary>
	/// The lazy collection that represents a redis dataset
	/// </summary>
	/// <typeparam name="T">The type of object in the collection</typeparam>
	public class RedisLazyCollection<T> : BaseLazyCollection<T>
	{
		/// <param name="source">The result of the query that will be lazily loaded</param>
		/// <param name="service">The redis service that made the query</param>
		public RedisLazyCollection(IEnumerable<T> source, RedisCachingService service)
			: base(source)
		{
			Service = service;
			Source = _source;
		}

		/// <summary>
		/// The result of the query that will be lazily loaded
		/// </summary>
		protected virtual IEnumerable<T> Source { get; set; }

		/// <summary>
		/// The redis service that made the query
		/// </summary>
		protected virtual RedisCachingService Service { get; }

		// TODO: slow and bad... is there a better way?
		/// <summary>
		/// DON'T USE THIS,	Gets the count of the query, likely slow
		/// </summary>
		/// <returns>The count of the result of the query, likely slow</returns>
		public override int Count => Source.Count();

		/// <summary>
		/// If the redis set is readonly, always false
		/// </summary>
		/// <returns>If the redis set is readonly, always false</returns>
		public override bool IsReadOnly => false;

		public override void Add(T item)
		{
			IRedisCachedObject castedItem = GetCastedItem(item);
			// TODO: this needs to add to the locally stored value as well
			Service.Cache(castedItem.Id, item);
		}

		/// <summary>
		/// DON'T USE THIS,	Gets the result of the query and removes them one by one
		/// </summary>
		public override void Clear()
		{
			while (Source.AnySafe())
			{
				bool removed = Remove(Source.FirstOrDefault());
				if (!removed)
				{
					Source = Source.Skip(1);
				}
			}
		}

		/// <summary>
		/// DON'T USE THIS,	Checks if the result set of query contains the item
		/// </summary>
		/// <param name="item">The item that the result set may or may not contain</param>
		/// <returns>If the result set contains the item</returns>
		public override bool Contains(T item)
		{
			return Source.Contains(item);
		}

		// slow and bad, but no better way
		/// <summary>
		/// DON'T USE THIS, Copies the result set to the array
		/// </summary>
		/// <param name="array">The array to copy to</param>
		/// <param name="arrayIndex">The index of the array</param>
		public override void CopyTo(T[] array, int arrayIndex)
		{
			Source.ToArray().CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes an item from redis
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <returns>If the item was removed</returns>
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

		/// <summary>
		/// Removes an item from the collection
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <param name="throwException">If an exception should be thrown when the item can not be cached</param>
		/// <exception cref="System.InvalidOperationException">Unable to cast the item to a
		/// <see cref="StandardDot.Caching.Redis.Abstract.IRedisCachedObject" /></exception>
		/// <returns>If the item was removed</returns>
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