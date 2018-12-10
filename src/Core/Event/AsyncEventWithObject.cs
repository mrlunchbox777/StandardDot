// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

using System;

namespace StandardDot.Core.Event
{
	public class AsyncEvent<T> : AsyncEvent<object, T>
		where T : EventArgs
	{ }
}