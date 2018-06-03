using System.Collections.Generic;

namespace Abstract.CoreServices
{
    public interface IPaginated<T> : IEnumerable<IPage<T>>
    {
         IEnumerable<T> Collate();

         void Reset();

         IPage<T> GetPage(int pageIndex);

         IPage<T> Current { get; }

         int PageSize { get; }

         int CurrentPageIndex { get; }

         IPage<T> GetNext();
    }
}