using System.Collections;
using System.Collections.Generic;

namespace Abstract.CoreServices
{
    public abstract class PageBase<T> : IPage<T>
    {
        public PageBase(IEnumerable<T> source, IPaginated<T> parentCollection, int pageIndex)
        {
            Source = source;
            ParentCollection = parentCollection;
            PageIndex = pageIndex;
        }

        protected virtual IEnumerable<T> Source { get; }

        public virtual IPaginated<T> ParentCollection { get; }

        public virtual IPage<T> GetNext()
        {
            return ParentCollection.GetPage(PageIndex + 1);
        }

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