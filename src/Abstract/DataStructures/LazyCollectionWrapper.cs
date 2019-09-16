using System.Collections.Generic;

namespace StandardDot.Abstract.DataStructures
{
	/// <summary>
	/// A wrapper that allows a collection to treated as if it were lazily enumerated
	/// </summary>
	/// <typeparam name="T">The type of object in the collection</typeparam>
	public class LazyCollectionWrapper<T> : BaseLazyCollection<T>
	{
		/// <param name="source">The collection to make lazy</param>
		public LazyCollectionWrapper(ICollection<T> source)
			: base(source)
		{ }

		/// <summary>
		/// The collection that back the lazy operation;
		/// </summary>
		protected ICollection<T> SourceCollection
		{
			get
			{
				if (_source == null)
				{
					_source = new List<T>();
				}
				return (ICollection<T>)_source;
			}
		}

		/// <summary>
		/// Gets the count for the source that has been enumerated
		/// </summary>
		/// <returns>The count of items that have been enumerated when called</returns>
		public override int Count => SourceCollection.Count;

		/// <summary>
		/// If the source is readonly
		/// </summary>
		/// <returns>If the source is readonly</returns>
		public override bool IsReadOnly => SourceCollection.IsReadOnly;

		/// <summary>
		/// Adds an item to the collection
		/// </summary>
		/// <param name="item">The item to add</param>
		public override void Add(T item)
		{
			SourceCollection.Add(item);
		}


		/// <summary>
		/// Clears the source collection
		/// </summary>
		public override void Clear()
		{
			SourceCollection.Clear();
		}

		/// <summary>
		/// If the source enumerated so far contains the item
		/// </summary>
		/// <param name="item">The item to check for</param>
		/// <returns>If the source is contains the item</returns>
		public override bool Contains(T item)
		{
			return SourceCollection.Contains(item);
		}

		/// <summary>
		/// Copies the source enumerated so far to the array
		/// </summary>
		/// <param name="array">The array to copy to</param>
		/// <param name="arrayIndex">The index of the array</param>
		public override void CopyTo(T[] array, int arrayIndex)
		{
			SourceCollection.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes an item from the collection
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <returns>If the item was removed</returns>
		public override bool Remove(T item)
		{
			return SourceCollection.Remove(item);
		}

		/// <summary>
		/// Returns the enumerator of the source collection
		/// </summary>
		/// <returns>If the item was removed</returns>
		public override IEnumerator<T> GetEnumerator()
		{
			return SourceCollection.GetEnumerator();
		}
	}
}
