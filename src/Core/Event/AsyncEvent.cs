using System;
using System.Threading.Tasks;

namespace StandardDot.Core.Event
{
	/// <summary>
	/// Async Event that where the sender type is <see cref="object" />
	/// and the args type is <see cref="EventArgs" />
	/// </summary>
	/// <typaram name ="T">The type of the sender</typeparam>
	public class AsyncEvent : AsyncEvent<object, EventArgs>
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