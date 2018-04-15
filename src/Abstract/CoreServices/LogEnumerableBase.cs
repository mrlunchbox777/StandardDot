using System;
using System.Collections;
using System.Collections.Generic;
using StandardDot.Dto.CoreServices;
using StandardDot.Dto.Exception;
using StandardDot.Enums;

namespace StandardDot.Abstract.CoreServices
{
    /// <summary>
    /// A Base Enumerator to get logs
    /// </summary>
    /// <typeparam name="T">The target type for the logs (must be serializable), should typically be object</typeparam>
    public abstract class LogEnumerableBase<T> : ILogEnumerable<T>
        where T : new()
    {
        /// <param name="source">The source that the enumerable should represent</param>
        public LogEnumerableBase(IEnumerable<Log<T>> source)
        {
            Source = source;
        }

        /// <param name="source">The source that the enumerable should represent</param>
        public LogEnumerableBase(ILogEnumerable<T> source)
        {
            Source = source;
        }

        protected IEnumerable<Log<T>> Source { get; }

        public virtual IEnumerator<Log<T>> GetEnumerator()
        {
            return Source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}