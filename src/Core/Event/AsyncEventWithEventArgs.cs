using System;
using System.Threading.Tasks;

namespace StandardDot.Core.Event
{
	/// <summary>
	/// Async Event that allows selection of the sender type.
	/// The args type is <see cref="EventArgs" />
	/// </summary>
	/// <typaram name ="T">The type of the sender</typeparam>
	public class AsyncEventWithArgs<T> : AsyncEvent<T, EventArgs>
	{
		/// <summary>
		/// No logging action called upon exceptions
		/// </summary>
		public AsyncEventWithArgs()
			: this(null)
		{ }

		/// <param name="loggingAction">The logging action to call with upon exceptions</param>
		public AsyncEventWithArgs(Func<Exception, Task> loggingAction)
			: base (loggingAction)
		{ }
	}
}