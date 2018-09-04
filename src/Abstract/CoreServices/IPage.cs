using System.Collections.Generic;

namespace Abstract.CoreServices
{
    /// <summary>
    /// An enumerable that represents a subset of larger enumerable
    /// </summary>
    /// <typeparam name="T">The type of items in the enumerable</typeparam>
    public interface IPage<T> : IEnumerable<T>
    {
        /// <summary>
        /// The enumerable that this page is a subset of
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable</typeparam>
        IPaginated<T> ParentCollection { get; }

        /// <summary>
        /// The next page in the larger enumerable
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable</typeparam>
        IPage<T> GetNext();

        /// <summary>
        /// The index of the current page
        /// </summary>
        int PageIndex { get; }
    }
}