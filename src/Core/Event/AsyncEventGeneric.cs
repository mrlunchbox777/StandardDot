using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace StandardDot.Core.Event
{
	public class AsyncEvent<T, Te>
		where Te : EventArgs
	{
		private event Func<T, Te, Task> asyncEvent;

		internal IList<Func<T, Te, Task>> SubscriberItems = new List<Func<T, Te, Task>>();

		bool _unaddedFuncs = true;

		private Task[] GetTasks(T sender, Te args)
		{
			if (_unaddedFuncs)
			{
				ClearDelegates();
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
				handlerTasks[i] = ((Func<T, Te, Task>)SubscriberItems[i])(sender, args);
			}

			return handlerTasks;
		}

		internal void ClearDelegates()
		{
			asyncEvent = null;
		}

		public async Task Raise(T sender, Te args)
		{
			Task[] handlerTasks = GetTasks(sender, args);

			await Task.WhenAll(handlerTasks);
		}

		public void Add(Func<T, Te, Task> subscriber)
		{
			_unaddedFuncs = true;
			SubscriberItems.Add(subscriber);
		}

		#region Event Implementation
		public List<Func<T, Te, Task>> GetInvocationList() => SubscriberItems.ToList();

		public Task Invoke(T sender, Te args) => Raise(sender, args);

		public MethodInfo Method => asyncEvent.Method;

		public object Target => asyncEvent.Target;

		public IAsyncResult BeginInvoke(T sender, Te args, AsyncCallback callback, object @object)
		{
			GetTasks(sender, args);
			return asyncEvent.BeginInvoke(sender, args, callback, @object);
		}

		public object DynamicInvoke(params object[] args)
		{
			GetTasks((T)args[0], args[1] as Te);
			return asyncEvent.DynamicInvoke(args);
		}

		public Task EndInvoke(IAsyncResult result)
		{
			return asyncEvent.EndInvoke(result);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			asyncEvent.GetObjectData(info, context);
		}
		#endregion Event Implementation

		public static AsyncEvent<T, Te> operator +(AsyncEvent<T, Te> subscriber1, AsyncEvent<T, Te> subscriber2)
		{
			foreach (Func<T, Te, Task> current in subscriber2.SubscriberItems)
			{
				subscriber1.Add(current);
			}
			return subscriber1;
		}

		public static explicit operator Func<T, Te, Task>(AsyncEvent<T, Te> asyncEvent)
		{
			if (asyncEvent == null)
			{
				return (x, y) => Task.Factory.StartNew(() => {});
			}
			return (x, y) => asyncEvent.Raise(x, y);
		}

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