using System.Collections.Generic;

namespace Abstract.CoreServices
{
    public interface IPage<T> : IEnumerable<T>
    {
         IPaginated<T> ParentCollection { get; }

         IPage<T> GetNext { get; }

         int PageIndex { get; }
    }
}