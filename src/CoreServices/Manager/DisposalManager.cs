using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StandardDot.CoreServices.Manager
{
	public class DisposalManager : IDisposable
	{
		private ConcurrentDictionary<Guid, IDisposable> _items = new ConcurrentDictionary<Guid, IDisposable>();
		private bool _disposed = false;

		public async Task RegisterIDisposable(IDisposable target)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(DisposalManager));
			}
			while(!_items.TryAdd(Guid.NewGuid(), target))
			{
				await Task.Delay(1);
			}
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
			while (_items.Any())
			{
				KeyValuePair<Guid, IDisposable> holder = _items.FirstOrDefault();
				if (holder.Key == default(Guid))
				{
					continue;
				}
				_items[holder.Key] = null;
				_items.TryRemove(holder.Key, out IDisposable value);
				try
				{
					holder.Value?.Dispose();
				}
				catch (ObjectDisposedException)
				{
					// If it's already disposed that is ok
				}
			}
			_disposed = true;
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