using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Abstract.CoreServices
{
    /// <summary>
    /// An enumerable of fixed size subsets
    /// </summary>
    /// <typeparam name="T">The type of items in the enumerable</typeparam>
    public abstract class PaginatedBase<T> : IPaginated<T>
    {
        /// <param name="source">The enumerable to paginate</param>
        /// <param name="pageSize">The size of each page</param>
        public PaginatedBase(IEnumerable<T> source, int pageSize)
        {
            Source = source;
            PageSize = pageSize;
            Reset();
        }

        /// <summary>
        /// The enumerable to paginate
        /// </summary>
        protected virtual IEnumerable<T> Source { get; }

        /// <summary>
        /// Gets the current page
        /// </summary>
        public virtual IPage<T> Current
        {
            get
            {
                if (CurrentPageIndex == -1)
                {
                    return null;
                }
                return CreatePage(Source.Skip(CurrentPageIndex * PageSize).Take(PageSize), CurrentPageIndex);
            }
        }

        /// <summary>
        /// Creates a page from a subset and it's index
        /// </summary>
        /// <param name="source">The source that the page will represent</param>
        /// <param name="pageIndex">The index of the page to get</param>
        protected abstract PageBase<T> CreatePage(IEnumerable<T> source, int pageIndex);

        /// <summary>
        /// The size of pages
        /// </summary>
        public virtual int PageSize { get; }

        /// <summary>
        /// The index of the current page
        /// </summary>
        private int _currentPageIndex = 0;

        /// <summary>
        /// Gets the index of the current page
        /// </summary>
        public virtual int CurrentPageIndex
        {
            get => _currentPageIndex;
            private set { _currentPageIndex = value; }
        }

        /// <summary>
        /// Gets the next page
        /// </summary>
        public virtual IPage<T> GetNext()
        {
            CurrentPageIndex++;
            return Current;
        }

        /// <summary>
        /// Combines all subsets into a single enumerable
        /// </summary>
        public virtual IEnumerable<T> Collate()
        {
            return Source;
        }

        /// <summary>
        /// Resets the current page index
        /// </summary>
        public virtual void Reset()
        {
            CurrentPageIndex = -1;
        }

        /// <summary>
        /// Gets the page at the pageIndex
        /// </summary>
        /// <param name="pageIndex">The index of the page to get</param>
        public virtual IPage<T> GetPage(int pageIndex)
        {
            if (pageIndex < 0)
            {
                return null;
            }

            return CreatePage(Source.Skip(pageIndex * PageSize).Take(PageSize), pageIndex);
        }

        public virtual IEnumerator<IPage<T>> GetEnumerator()
        {
            Reset();
            while ((Current?.Any() ?? false) || CurrentPageIndex == -1)
            {
                yield return GetNext();
            }
            Reset();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}