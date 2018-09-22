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
	/// <typeparam name="T">The target type for the logs (must be serializable)</typeparam>
	public interface ILogEnumerable<T> : IEnumerable<Log<T>>
		where T : new()
	{ }
}