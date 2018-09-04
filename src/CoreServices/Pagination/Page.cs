using System.Collections.Generic;
using Abstract.CoreServices;

namespace CoreServices.Pagination
{
    /// <summary>
    /// An enumerable that represents a subset of larger enumerable
    /// </summary>
    /// <typeparam name="T">The type of items in the enumerable</typeparam>
    public class Page<T> : PageBase<T>
    {
        /// <param name="source">The subset that the page represents</param>
        /// <param name="parentCollection">The enumerable that the subset came from</param>
        /// <param name="pageIndex">The index of the subset</param>
        public Page(IEnumerable<T> source, IPaginated<T> parentCollection, int pageIndex)
            : base(source, parentCollection, pageIndex)
        { }
    }
}