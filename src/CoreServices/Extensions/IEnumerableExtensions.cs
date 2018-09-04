using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Abstract.CoreServices;
using CoreServices.Pagination;

namespace StandardDot.CoreServices.Extensions
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
        public static IPaginated<T> Paginate<T>(this IEnumerable<T> source, int pageSize)
        {
            if (pageSize < 0)
            {
                throw new InvalidDataException("pageSize must be greater than 0.");
            }

            if (source == null)
            {
                return null;
            }

            return new Paginated<T>(source, pageSize);
        }
    }
}