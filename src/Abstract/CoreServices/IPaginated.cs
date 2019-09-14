using System.Collections.Generic;

namespace Abstract.CoreServices
{
	/// <summary>
	/// An enumerable of fixed size subsets
	/// </summary>
	/// <typeparam name="T">The type of items in the enumerable</typeparam>
	public interface IPaginated<T> : IEnumerable<IPage<T>>
	{
		/// <summary>
		/// Combines all subsets into a single enumerable
		/// </summary>
		IEnumerable<T> Collate();

		/// <summary>
		/// Resets the current page index
		/// </summary>
		void Reset();

		/// <summary>
		/// Gets the page at the pageIndex
		/// </summary>
		/// <param name="pageIndex">The index of the page to get</param>
		IPage<T> GetPage(int pageIndex);

		/// <summary>
		/// Gets the current page
		/// </summary>
		IPage<T> Current { get; }

		/// <summary>
		/// The size of pages
		/// </summary>
		int PageSize { get; }

		/// <summary>
		/// Gets the index of the current page
		/// </summary>
		int CurrentPageIndex { get; }

		/// <summary>
		/// Gets the next page
		/// </summary>
		IPage<T> GetNext();
	}
}