using System.Collections.Generic;
using Abstract.CoreServices;

namespace CoreServices.Pagination
{
    public class Paginated<T> : PaginatedBase<T>
    {
        public Paginated(IEnumerable<T> source, int pageSize)
            : base(source, pageSize)
        { }

        protected override PageBase<T> CreatePage(IEnumerable<T> source, int pageIndex)
        {
            return new Page<T>(source, this, pageIndex);
        }
    }
}