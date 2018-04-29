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
    public abstract class LogBaseEnumerableBase : ILogBaseEnumerable
    {
        /// <param name="source">The source that the enumerable should represent</param>
        public LogBaseEnumerableBase(IEnumerable<LogBase> source)
        {
            Source = source;
        }

        /// <param name="source">The source that the enumerable should represent</param>
        public LogBaseEnumerableBase(ILogBaseEnumerable source)
        {
            Source = source;
        }

        protected IEnumerable<LogBase> Source { get; }

        public virtual IEnumerator<LogBase> GetEnumerator()
        {
            return Source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}