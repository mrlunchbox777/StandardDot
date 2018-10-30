using System;
using System.Collections.Concurrent;
using System.Linq;

namespace StandardDot.CoreServices.Manager
{
	public class DisposalManager : IDisposable
	{
		private ConcurrentBag<IDisposable> _items = new ConcurrentBag<IDisposable>();
		private bool _disposed = false;

		public void RegisterIDisposable(IDisposable target)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(DisposalManager));
			}
			_items.Add(target);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
			{
				return;
			}
			if (!(_items?.Any() ?? false))
			{
				return;
			}
			foreach (var item in _items)
			{
				item?.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~DisposalManager()
		{
			Dispose(false);
		}
	}
}