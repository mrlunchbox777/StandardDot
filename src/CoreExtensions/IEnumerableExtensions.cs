using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace StandardDot.CoreExtensions
{
    /// <summary>
    /// Extensions for IEnumerables.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Pagination for any IEnumerable. Should be ORM safe.
        /// </summary>
        /// <param name="source">The IEnumerable</param>
        /// <param name="pageSize">The number of T on a page</param>
        /// <param name="page">The 0 based page to look at</param>
        /// <typeparam name="T">The type in the IEnumerable</typeparam>
        /// <returns>The object represented by the jsonString.</returns>
        public static TE Paginate<T, TE>(this TE source, int pageSize, int page)
            where TE: IEnumerable<T>
        {
            if (pageSize < 0 || page < 0)
            {
                throw new InvalidDataException("page and pageSize must be greater than or equal to 0.");
            }

            if (source == null)
            {
                return source;
            }

            return (TE)source.Skip(page * pageSize).Take(pageSize);
        }
    }
}