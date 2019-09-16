using System.Collections.Generic;
using Abstract.CoreServices;

namespace CoreServices.Pagination
{
	/// <summary>
	/// An enumerable of fixed size subsets
	/// </summary>
	/// <typeparam name="T">The type of items in the enumerable</typeparam>
	public class Paginated<T> : PaginatedBase<T>
	{
		/// <param name="source">The enumerable to paginate</param>
		/// <param name="pageSize">The size of each page</param>
		public Paginated(IEnumerable<T> source, int pageSize)
			: base(source, pageSize)
		{ }

		/// <summary>
		/// Creates a page from a subset and it's index
		/// </summary>
		/// <param name="source">The source that the page will represent</param>
		/// <param name="pageIndex">The index of the page to get</param>
		protected override PageBase<T> CreatePage(IEnumerable<T> source, int pageIndex)
		{
			return new Page<T>(source, this, pageIndex);
		}
	}
}