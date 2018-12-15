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
			LoggingAction = loggingAction;
		}

		protected Func<Exception, Task> LoggingAction { get; }

		private event Func<T, Te, Task> asyncEvent;

		private IList<Func<T, Te, Task>> _subscriberItems = new List<Func<T, Te, Task>>();

		internal IList<Func<T, Te, Task>> SubscriberItems { get => _subscriberItems; set => _subscriberItems = value; }

		private bool _unaddedFuncs = true;

		/// <summary>
		/// Gets the tasks for the underlying event for invocation
		/// </summary>
		/// <param name="sender">The sender of the invocation</param>
		/// <param name="args">The arguments for the invocation</param>
		/// <returns>An array of tasks for invocation</returns>
		private async Task<List<Task>> GetTasks(T sender, Te args,
			Func<Exception, T, Te, Task<bool>> exceptionHandler)
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
				return new List<Task>();
			}

			List<Task> returnTasks = new List<Task>(SubscriberItems.Count);

			for (int i = 0; i < SubscriberItems.Count; i++)
			{
				await Task.Factory.StartNew(async () =>
				{
					try
					{
						Task current = SubscriberItems[i](sender, args);
						if (current != null)
						{
							returnTasks.Add(current);
						}
					}
					catch (Exception ex)
					{
						await ExceptionHandler(ex, sender, args, exceptionHandler);
					}
				});
			}

			return returnTasks;
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
			await Raise(sender, args, null);
		}

		/// <summary>
		/// Invokes the event and calls all subscribers
		/// </summary>
		/// <param name="sender">The sender of the invocation</param>
		/// <param name="args">The arguments for the invocation</param>
		/// <returns>The task that represents the sending of the event</returns>
		public async Task Raise(T sender, Te args,
			Func<Exception, T, Te, Task<bool>> exceptionHandler = null)
		{
			List<Task> handlerTasks = await GetTasks(sender, args, exceptionHandler);

			if (!(handlerTasks?.Any() ?? false))
			{
				return;
			}

			try
			{
				await Task.WhenAll(handlerTasks);
			}
			catch (Exception ex)
			{
				await ExceptionHandler(ex, sender, args, exceptionHandler);
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

		/// <summary>
		/// Logs and Handles the exception if applicable, throws by default
		/// </summary>
		/// <param name="sender">The sender of the invocation</param>
		/// <param name="args">The arguments for the invocation</param>
		/// <returns>The task that represents the handling of exceptions</returns>
		protected async Task ExceptionHandler(Exception exception, T sender = default(T), Te args = null,
			Func<Exception, T, Te, Task<bool>> exceptionHandler = null)
		{
			if (LoggingAction != null)
			{
				await LoggingAction(exception);
			}
			if (exceptionHandler != null)
			{
				if (await exceptionHandler(exception, sender, args))
				{
					throw new AggregateException("Async Event exception. See InnerException for details", exception);
				}
			}
			else
			{
				throw new AggregateException("Async Event exception. See InnerException for details", exception);
			}
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
		public async Task Invoke(T sender, Te args) => await Raise(sender, args);

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
		public async Task<IAsyncResult> BeginInvoke(T sender, Te args, AsyncCallback callback, object @object)
		{
			await GetTasks(sender, args, null);
			return asyncEvent.BeginInvoke(sender, args, callback, @object);
		}

		/// <summary>
		/// Calls Raise
		/// </summary>
		/// <param name="sender">The sender of the invocation</param>
		/// <param name="args">The arguments for the invocation</param>
		/// <param name="callback">The callback for the invocation</param>
		/// <param name="@object">The object for the invocation</param>
		/// <returns>The <see cref="IAsyncResult" /> that represents the invocation</returns>
		public IAsyncResult BeginInvoke(T sender, Te args, AsyncCallback callback, object @object,
			Func<Exception, T, Te, Task<bool>> exceptionHandler)
		{
			return Raise(sender, args, exceptionHandler);
		}

		/// <summary>
		/// Starts the dynamic Invocation of the event
		/// </summary>
		/// <param name="params">All needed params for the invocation of the event</param>
		/// <returns>The <see cref="object" /> that represents the invocation</returns>
		public async Task<object> DynamicInvoke(params object[] args)
		{
			Func<Exception, T, Te, Task<bool>> exceptionHandler =
				args.FirstOrDefault(x => x is Func<Exception, T, Te, Task<bool>>
					|| (x != null && typeof(AsyncEvent<T, Te>).IsAssignableFrom(x.GetType())))
				as Func<Exception, T, Te, Task<bool>>;
			IEnumerable<object> newArgs = args.Where(x => !(x is Func<Exception, T, Te, Task<bool>>));
			await GetTasks((T)args[0], args[1] as Te, exceptionHandler);
			return asyncEvent.DynamicInvoke(newArgs);
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
		/// Calls <c>GetObjectData(info, context)</c> on the underlying event
		/// </summary>
		/// <param name="info">The info for serialization</param>
		/// <param name="result">The context for serialization</param>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			asyncEvent.GetObjectData(info, context);
		}

		/// <summary>
		/// Calls <c>GetObjectData(info, context)</c> on the underlying event
		/// </summary>
		/// <param name="info">The info for serialization</param>
		/// <param name="result">The context for serialization</param>
		public async Task GetObjectData(SerializationInfo info, StreamingContext context,
			Func<Exception, T, Te, Task<bool>> exceptionHandler)
		{
			await GetTasks(default(T), null, exceptionHandler);
			try
			{
				asyncEvent.GetObjectData(info, context);
			}
			catch (Exception ex)
			{
				await ExceptionHandler(ex, default(T), null, exceptionHandler);
			}
		}
		#endregion Event Implementation

		#region Operators
		/// <summary>
		/// Adds an <see cref="AsyncEvent" /> or 1+ <see cref="Func<T, Te, Task>" />(s) to an <see cref="AsyncEvent" />
		/// </summary>
		/// <param name="result">The primary <see cref="IAsyncResult" /></param>
		/// <param name="result">The secondary <see cref="IAsyncResult" /></param>
		/// <returns>The primary <see cref="AsyncEvent" /> with the addtional subscribers</returns>
		public static AsyncEvent<T, Te> operator +(AsyncEvent<T, Te> subscriber1, AsyncEvent<T, Te> subscriber2)
		{
			if (subscriber1 == null)
			{
				subscriber1 = new AsyncEvent<T, Te>();
			}
			if (!(subscriber2?.SubscriberItems.Any() ?? false))
			{
				return subscriber1;
			}
			foreach (Func<T, Te, Task> current in subscriber2.SubscriberItems)
			{
				subscriber1.Add(current);
			}
			return subscriber1;
		}

		/// <summary>
		/// Converts an <see cref="AsyncEvent" /> to a <see cref="Func<T, Te, Task>" />
		/// </summary>
		/// <param name="asyncEvent">The <see cref="AsyncEvent" /> that will have <c>Raise(x,y)</c> invoked by the <see cref="Func<T, Te, Task>" /></param>
		/// <returns>The <see cref="Func<T, Te, Task>" /> that represents the invocation of the <see cref="AsyncEvent" /></returns>
		public static explicit operator Func<T, Te, Task>(AsyncEvent<T, Te> asyncEvent)
		{
			if (asyncEvent == null)
			{
				return (x, y) => Task.Factory.StartNew(() => { });
			}
			return (x, y) => asyncEvent.Raise(x, y);
		}

		/// <summary>
		/// Converts a <see cref="Func<T, Te, Task>" /> to an <see cref="AsyncEvent" />
		/// </summary>
		/// <param name="subscriberFunction">The <see cref="Func<T, Te, Task>" /> that will be invoked by the <see cref="AsyncEvent" /></param>
		/// <returns>The <see cref="AsyncEvent" /> that will have a subscriber <see cref="Func<T, Te, Task>" /></returns>
		public static implicit operator AsyncEvent<T, Te>(Func<T, Te, Task> subscriberFunction)
		{
			AsyncEvent<T, Te> current = new AsyncEvent<T, Te>();
			if (subscriberFunction != null)
			{
				current.Add(subscriberFunction);
			}
			return current;
		}
		#endregion
	}
}