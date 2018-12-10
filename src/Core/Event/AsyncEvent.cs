using System;
using System.Threading.Tasks;

namespace StandardDot.Core.Event
{
	public class AsyncEvent
	{
		private event Func<object, EventArgs, Task> asyncEvent;

		public async Task Raise()
		{
			Func<object, EventArgs, Task> handler = asyncEvent;

			if (handler == null)
			{
				return;
			}

			Delegate[] invocationList = GetDelegates();
			Task[] handlerTasks = new Task[invocationList.Length];

			for (int i = 0; i < invocationList.Length; i++)
			{
				handlerTasks[i] = ((Func<object, EventArgs, Task>)invocationList[i])(this, EventArgs.Empty);
			}

			await Task.WhenAll(handlerTasks);
		}

		private Delegate[] GetDelegates()
		{
			return asyncEvent.GetInvocationList();
		}

		public void AddSubscriber(Func<object, EventArgs, Task> subscriber)
		{
			asyncEvent += subscriber;
		}

		public static AsyncEvent operator +(AsyncEvent subscriber1, AsyncEvent subscriber2)
		{
			subscriber1.AddSubscriber(subscriber2);
			return subscriber1;
		}

		public static implicit operator Func<object, EventArgs, Task>(AsyncEvent asyncEvent)
		{
			Delegate[] invocationList = asyncEvent.GetDelegates();
			return async (x, y) =>
			{
				await asyncEvent.Raise();
			};
		}

		public static implicit operator AsyncEvent(Func<object, EventArgs, Task> asyncEvent)
		{
			AsyncEvent current = new AsyncEvent();
			current += asyncEvent;
			return current;
		}
	}
}