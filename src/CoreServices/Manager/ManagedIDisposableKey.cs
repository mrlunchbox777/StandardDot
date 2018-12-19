using System;

namespace StandardDot.CoreServices.Manager
{
	/// <summary>
	/// A Key for the Disposal Manager, it is basically a Guid with a callbefore and callback
	/// </summary>
	public class ManagedIDisposableKey
	{
		public Guid Id { get; set; }

		public event Action<ManagedIDisposableKey, IDisposable> Callback;

		public event Action<ManagedIDisposableKey, IDisposable> Callbefore;

		internal void TriggerCallbefore(IDisposable value)
		{
			Callbefore(this, value);
		}

		internal void TriggerCallback(IDisposable value)
		{
			Callback(this, value);
		}

		public override bool Equals(object obj)
		{
			return Id.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			return Id.ToString();
		}

		public static bool operator == (ManagedIDisposableKey item1, ManagedIDisposableKey item2)
		{
			if (item1 == null)
			{
				return item2 == null;
			}
			return item1.Equals(item2);
		}

		public static bool operator != (ManagedIDisposableKey item1, ManagedIDisposableKey item2)
		{
			return !(item1 == item2);
		}
	}
}