using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Abstract.CoreServices
{
    public abstract class PaginatedBase<T> : IPaginated<T>
    {
        public PaginatedBase(IEnumerable<T> source, int pageSize)
        {
            Source = source;
            PageSize = pageSize;
            Reset();
        }

        protected virtual IEnumerable<T> Source { get; }

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

        protected abstract PageBase<T> CreatePage(IEnumerable<T> source, int pageIndex);

        public virtual int PageSize { get; }

        private int _currentPageIndex = 0;

        public virtual int CurrentPageIndex
        {
            get => _currentPageIndex;
            private set { _currentPageIndex = value; }
        }

        public virtual IPage<T> GetNext
        {
            get
            {
                CurrentPageIndex++;
                return Current;
            }
        }

        public virtual IEnumerable<T> Collate()
        {
            return Source;
        }

        public virtual void Reset()
        {
            CurrentPageIndex = -1;
        }
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
            while ((Current?.Any() ?? false) && CurrentPageIndex != -1)
            {
                yield return GetNext;
            }
            Reset();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}