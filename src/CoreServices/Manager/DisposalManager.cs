using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StandardDot.CoreServices.Manager
{
	public class DisposalManager : AFinalDispose
	{
		private ConcurrentDictionary<Guid, IDisposable> _items = new ConcurrentDictionary<Guid, IDisposable>();

		public async Task RegisterIDisposable(IDisposable target)
		{
			if (Disposed)
			{
				throw new ObjectDisposedException(nameof(DisposalManager));
			}
			while(!_items.TryAdd(Guid.NewGuid(), target))
			{
				await Task.Delay(1);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing || Disposed)
			{
				base.Dispose(disposing);
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
			base.Dispose(disposing);
		}
	}
}