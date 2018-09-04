using System.Collections.Generic;
using Abstract.CoreServices;

namespace CoreServices.Pagination
{
    public class Page<T> : PageBase<T>
    {
        public Page(IEnumerable<T> source, IPaginated<T> parentCollection, int pageIndex)
            : base(source, parentCollection, pageIndex)
        { }
    }
}