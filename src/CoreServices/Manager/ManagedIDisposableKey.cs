using System;

namespace StandardDot.CoreServices.Manager
{
	public class ManagedIDisposableKey
	{
		public Guid Id { get; set; }

		public event Action<ManagedIDisposableKey, IDisposable> Callback;

		public event Action<ManagedIDisposableKey, IDisposable> Callbefore;

		internal void TriggerCallbefore(IDisposable value)
		{
			if (Callbefore != null)
			{
				Callbefore(this, value);
			}
		}

		internal void TriggerCallback(IDisposable value)
		{
			if (Callback != null)
			{
				Callback(this, value);
			}
		}

		public override bool Equals(object obj)
		{
			if (Id == null)
			{
				return obj == null;
			}
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

		public override int GetHashCode()
		{
			if (Id == null)
			{
				return Guid.Empty.GetHashCode();
			}
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			if (Id == null)
			{
				Guid.Empty.ToString();
			}
			return Id.ToString();
		}

		public string ToString(string format)
		{
			if (Id == null)
			{
				Guid.Empty.ToString(format);
			}
			return Id.ToString(format);
		}

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

		public static bool operator != (ManagedIDisposableKey item1, ManagedIDisposableKey item2)
		{
			return !(item1 == item2);
		}
	}
}