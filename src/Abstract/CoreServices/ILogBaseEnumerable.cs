using System;
using System.Collections;
using System.Collections.Generic;
using StandardDot.Dto.CoreServices;
using StandardDot.Dto.Exception;
using StandardDot.Enums;

namespace StandardDot.Abstract.CoreServices
{
    /// <summary>
    /// An Enumerator to get logs
    /// </summary>
    public interface ILogBaseEnumerable : IEnumerable<LogBase>
    {}
}