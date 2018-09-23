using System.Collections;
using System.Collections.Generic;

namespace Abstract.CoreServices
{
	/// <summary>
	/// An enumerable that represents a subset of larger enumerable
	/// </summary>
	/// <typeparam name="T">The type of items in the enumerable</typeparam>
	public abstract class PageBase<T> : IPage<T>
	{
		/// <param name="source">The subset that the page represents</param>
		/// <param name="parentCollection">The enumerable that the subset came from</param>
		/// <param name="pageIndex">The index of the subset</param>
		public PageBase(IEnumerable<T> source, IPaginated<T> parentCollection, int pageIndex)
		{
			Source = source;
			ParentCollection = parentCollection;
			PageIndex = pageIndex;
		}

		/// <summary>
		/// The subset that the page represents
		/// </summary>
		/// <typeparam name="T">The type of items in the enumerable</typeparam>
		protected virtual IEnumerable<T> Source { get; }

		/// <summary>
		/// The enumerable that this page is a subset of
		/// </summary>
		/// <typeparam name="T">The type of items in the enumerable</typeparam>
		public virtual IPaginated<T> ParentCollection { get; }

		/// <summary>
		/// The next page in the larger enumerable
		/// </summary>
		/// <typeparam name="T">The type of items in the enumerable</typeparam>
		public virtual IPage<T> GetNext()
		{
			return ParentCollection.GetPage(PageIndex + 1);
		}

		/// <summary>
		/// The index of the current page
		/// </summary>
		public virtual int PageIndex { get; }

		public virtual IEnumerator<T> GetEnumerator()
		{
			return Source.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}