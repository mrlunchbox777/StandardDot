using System.Collections.Generic;

namespace Abstract.CoreServices
{
    public interface IPage<T> : IEnumerable<T>
    {
         IPaginated<T> ParentCollection { get; }

         IPage<T> GetNext();

         int PageIndex { get; }
    }
}