using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace StandardDot.Core.Event
{
	/// <summary>
	/// Async Event that allows selection of the sender type
	/// and args type
	/// </summary>
	/// <typaram name ="T">The type of the sender</typeparam>
	/// <typaram name ="Te">The type of the args</typeparam>
	public class AsyncEvent<T, Te>
		where Te : EventArgs
	{
		/// <summary>
		/// No logging action called upon exceptions
		/// </summary>
		public AsyncEvent()
			: this(null)
		{ }

		/// <param name="loggingAction">The logging action to call with upon exceptions</param>
		public AsyncEvent(Func<Exception, Task> loggingAction)
		{
			_loggingAction = loggingAction;
		}

		private Func<Exception, Task> _loggingAction;

		private event Func<T, Te, Task> asyncEvent;

		internal IList<Func<T, Te, Task>> SubscriberItems = new List<Func<T, Te, Task>>();

		private bool _unaddedFuncs = true;

		/// <summary>
		/// Gets the tasks for the underlying event for invocation
		/// </summary>
		/// <param name="sender">The sender of the invocation</param>
		/// <param name="args">The arguments for the invocation</param>
		/// <returns>An array of tasks for invocation</returns>
		private Task[] GetTasks(T sender, Te args)
		{
			if (_unaddedFuncs)
			{
				ClearDelegatesFromEventLiteral();
				foreach (Func<T, Te, Task> subscriber in SubscriberItems)
				{
					asyncEvent += subscriber;
				}
				_unaddedFuncs = false;
			}
			Func<T, Te, Task> handler = asyncEvent;

			if (handler == null)
			{
				return new Task[0];
			}

			Task[] handlerTasks = new Task[SubscriberItems.Count];

			for (int i = 0; i < SubscriberItems.Count; i++)
			{
				handlerTasks[i] = Task.Factory.StartNew(() => {
					try
					{
						SubscriberItems[i](sender, args);
					}
					catch(Exception ex)
					{
						_loggingAction.Invoke(ex);
					}
				});
			}

			return handlerTasks;
		}

		/// <summary>
		/// Clears all subscribers fromt the event literal
		/// </summary>
		internal void ClearDelegatesFromEventLiteral()
		{
			asyncEvent = null;
		}


		/// <summary>
		/// Invokes the event and calls all subscribers
		/// </summary>
		/// <param name="sender">The sender of the invocation</param>
		/// <param name="args">The arguments for the invocation</param>
		/// <returns>The task that represents the sending of the event</returns>
		public async Task Raise(T sender, Te args)
		{
			Task[] handlerTasks = GetTasks(sender, args);

			try
			{
				await Task.WhenAll(handlerTasks);
			}
			catch (Exception ex)
			{
				if (_loggingAction != null)
				{
					await _loggingAction(ex);
				}
				throw;
			}
		}

		/// <summary>
		/// Adds a subscriber to the event
		/// </summary>
		/// <param name="subscriber">The subscriber to add to the event</param>
		public void Add(Func<T, Te, Task> subscriber)
		{
			_unaddedFuncs = true;
			SubscriberItems.Add(subscriber);
		}

		#region Event Implementation
		/// <summary>
		/// Gets the functions for the event
		/// </summary>
		/// <returns>A <see cref="List<Func<T, Te, Task>>" /> of the underlying event</returns>
		public List<Func<T, Te, Task>> GetInvocationList() => SubscriberItems.ToList();

		/// <summary>
		/// Invokes the event
		/// </summary>
		/// <param name="sender">The sender of the invocation</param>
		/// <param name="args">The arguments for the invocation</param>
		/// <returns>A <see cref="Task" /> that reprensents the completion of invocation</returns>
		public Task Invoke(T sender, Te args) => Raise(sender, args);

		/// <summary>
		/// Gets the <see cref="MethodInfo" /> for the underlying event
		/// </summary>
		/// <returns>The <see cref="MethodInfo" /> for the underlying event</returns>
		public MethodInfo Method => asyncEvent.Method;

		/// <summary>
		/// Gets the <c>event.Target</c> for the underlying event
		/// </summary>
		/// <returns>The <c>event.Target</c> object for the underlying event</returns>
		public object Target => asyncEvent.Target;

		/// <summary>
		/// Starts the Invocation of the event
		/// </summary>
		/// <param name="sender">The sender of the invocation</param>
		/// <param name="args">The arguments for the invocation</param>
		/// <param name="callback">The callback for the invocation</param>
		/// <param name="@object">The object for the invocation</param>
		/// <returns>The <see cref="IAsyncResult" /> that represents the invocation</returns>
		public IAsyncResult BeginInvoke(T sender, Te args, AsyncCallback callback, object @object)
		{
			GetTasks(sender, args);
			return asyncEvent.BeginInvoke(sender, args, callback, @object);
		}

		/// <summary>
		/// Starts the dynamic Invocation of the event
		/// </summary>
		/// <param name="params">All needed params for the invocation of the event</param>
		/// <returns>The <see cref="object" /> that represents the invocation</returns>
		public object DynamicInvoke(params object[] args)
		{
			GetTasks((T)args[0], args[1] as Te);
			return asyncEvent.DynamicInvoke(args);
		}

		/// <summary>
		/// Watches for the end of the invocation
		/// </summary>
		/// <param name="result">The <see cref="IAsyncResult" /> that represents the invocation</param>
		/// <returns>The <see cref="Task" /> that represents the end of the invocation</returns>
		public Task EndInvoke(IAsyncResult result)
		{
			return asyncEvent.EndInvoke(result);
		}

		/// <summary>
		/// Watches for the end of the invocation
		/// </summary>
		/// <param name="info">The info for serialization</param>
		/// <param name="result">The context for serialization</param>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			asyncEvent.GetObjectData(info, context);
		}
		#endregion Event Implementation

		/// <summary>
		/// Adds an <see cref="AsyncEvent" /> or 1+ <see cref="Func<T, Te, Task>" />(s) to an <see cref="AsyncEvent" />
		/// </summary>
		/// <param name="result">The primary <see cref="IAsyncResult" /></param>
		/// <param name="result">The secondary <see cref="IAsyncResult" /></param>
		/// <returns>The primary <see cref="AsyncEvent" /> with the addtional subscribers</returns>
		public static AsyncEvent<T, Te> operator +(AsyncEvent<T, Te> subscriber1, AsyncEvent<T, Te> subscriber2)
		{
			foreach (Func<T, Te, Task> current in subscriber2.SubscriberItems)
			{
				subscriber1.Add(current);
			}
			return subscriber1;
		}

		/// <summary>
		/// Converts an <see cref="AsyncEvent" /> to a <see cref="Func<T, Te, Task>" />
		/// </summary>
		/// <param name="result">The <see cref="IAsyncResult" /> that will be invoked by the <see cref="Func<T, Te, Task>" /></param>
		/// <returns>The <see cref="Func<T, Te, Task>" /> that represents the invocation of the <see cref="AsyncEvent" /></returns>
		public static explicit operator Func<T, Te, Task>(AsyncEvent<T, Te> asyncEvent)
		{
			if (asyncEvent == null)
			{
				return (x, y) => Task.Factory.StartNew(() => {});
			}
			return (x, y) => asyncEvent.Raise(x, y);
		}

		/// <summary>
		/// Converts a <see cref="Func<T, Te, Task>" /> to an <see cref="AsyncEvent" />
		/// </summary>
		/// <param name="result">The <see cref="Func<T, Te, Task>" /> that will be invoked by the <see cref="IAsyncResult" /></param>
		/// <returns>The <see cref="AsyncEvent" /> that will have a subscriber <see cref="Func<T, Te, Task>" /></returns>
		public static implicit operator AsyncEvent<T, Te>(Func<T, Te, Task> asyncEvent)
		{
			AsyncEvent<T, Te> current = new AsyncEvent<T, Te>();
			if (asyncEvent != null)
			{
				current.Add(asyncEvent);
			}
			return current;
		}
	}
}