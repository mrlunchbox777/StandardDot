using System;

namespace StandardDot.CoreServices.Manager
{
	/// <summary>
	/// A Key for the Disposal Manager, it is basically a Guid with a callbefore and callback
	/// </summary>
	public class ManagedIDisposableKey
	{
		/// <summary>
		/// The underlying Guid that provides the unique Id
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The event that will be raised before disposal
		/// </summary>
		protected event Action<ManagedIDisposableKey, IDisposable, DisposalManager> Callbefore;

		/// <summary>
		/// The event that will be raised after disposal
		/// </summary>
		protected event Action<ManagedIDisposableKey, DisposalManager> Callback;

		/// <summary>
		/// Subscribes to the Callbefore
		/// </summary>
		/// <param name="subscriber">The subscriber to add to the Callbefore</param>
		public void SubscribeBefore(Action<ManagedIDisposableKey, IDisposable, DisposalManager> subscriber)
		{
			Callbefore += subscriber;
		}

		/// <summary>
		/// Subscribes to the Callback
		/// </summary>
		/// <param name="subscriber">The subscriber to add to the Callback</param>
		public void SubscribeAfter(Action<ManagedIDisposableKey, DisposalManager> subscriber)
		{
			Callback += subscriber;
		}

		/// <summary>
		/// Unsubscribes to the Callbefore
		/// </summary>
		/// <param name="subscriber">The subscriber to remove from the Callbefore</param>
		public void UnsubscribeBefore(Action<ManagedIDisposableKey, IDisposable, DisposalManager> subscriber)
		{
			Callbefore -= subscriber;
		}

		/// <summary>
		/// Unsubscribes to the Callback
		/// </summary>
		/// <param name="subscriber">The subscriber to remove from the Callback</param>
		public void UnsubscribeAfter(Action<ManagedIDisposableKey, DisposalManager> subscriber)
		{
			Callback -= subscriber;
		}

		/// <summary>
		/// Raise the Callbefore
		/// </summary>
		/// <param name="value">The IDisposable about to be disposed</param>
		/// <param name="manager">The <see cref="StandardDot.CoreServices.Manager.DisposalManager" /> that is disposing the value</param>
		internal void TriggerCallbefore(IDisposable value, DisposalManager manager)
		{
			if (Callbefore != null)
			{
				Callbefore(this, value, manager);
			}
		}

		/// <summary>
		/// Raise the Callback
		/// </summary>
		/// <param name="manager">The <see cref="StandardDot.CoreServices.Manager.DisposalManager" /> that disposed the value</param>
		internal void TriggerCallback(DisposalManager manager)
		{
			if (Callback != null)
			{
				Callback(this, manager);
			}
		}

		/// <summary>
		/// Checks for equality of the underlying Id
		/// </summary>
		/// <param name="obj">The object to check equality against</param>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return Id == null;
			}
			if (obj is Guid)
			{
				return Id == (Guid)obj;
			}
			if (obj is ManagedIDisposableKey)
			{
				return Id == ((ManagedIDisposableKey)obj).Id;
			}
			return Id.Equals(obj);
		}

		/// <summary>
		/// Gets the Hash Code of the underlying Id
		/// </summary>
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		/// <summary>
		/// Gets the string representation of the underlying Id
		/// </summary>
		public override string ToString()
		{
			return Id.ToString();
		}

		/// <summary>
		/// Gets the string representation of the underlying Id with formatting
		/// </summary>
		/// <param name="format">The format string to pass to the underlying Id</param>
		public string ToString(string format)
		{
			return Id.ToString(format);
		}

		/// <summary>
		/// Checks for equality of the underlying Id
		/// </summary>
		/// <param name="item1">The first object to check equality against</param>
		/// <param name="item2">The second object to check equality against</param>
		public static bool operator == (ManagedIDisposableKey item1, ManagedIDisposableKey item2)
		{
			object objItem1 = item1 as object;
			if (objItem1 == null && (item2 as object) == null)
			{
				return true;
			}
			if (objItem1 == null)
			{
				return item2.Equals(item1);
			}
			return item1.Equals(item2);
		}

		/// <summary>
		/// Checks for non-equality of the underlying Id
		/// </summary>
		/// <param name="item1">The first object to check non-equality against</param>
		/// <param name="item2">The second object to check non-equality against</param>
		public static bool operator != (ManagedIDisposableKey item1, ManagedIDisposableKey item2)
		{
			return !(item1 == item2);
		}
	}
}