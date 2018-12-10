// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

using System;
using System.Threading.Tasks;

namespace StandardDot.Core.Event
{
	/// <summary>
	/// Async Event that allows selection of the args type.
	/// The args sender is <see cref="object" />
	/// </summary>
	/// <typaram name ="T">The type of the sender</typeparam>
	public class AsyncEvent<T> : AsyncEvent<object, T>
		where T : EventArgs
	{
		/// <summary>
		/// No logging action called upon exceptions
		/// </summary>
		public AsyncEvent()
			: this(null)
		{ }

		/// <param name="loggingAction">The logging action to call with upon exceptions</param>
		public AsyncEvent(Func<Exception, Task> loggingAction)
			: base (loggingAction)
		{ }
	}
}