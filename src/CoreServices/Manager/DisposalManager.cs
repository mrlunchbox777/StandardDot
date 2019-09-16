using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StandardDot.CoreExtensions;

namespace StandardDot.CoreServices.Manager
{
	public class DisposalManager : AFinalDispose
	{
		public int retryLimit { get; set; } = 10;

		private ConcurrentDictionary<ManagedIDisposableKey, IDisposable> _items = new ConcurrentDictionary<ManagedIDisposableKey, IDisposable>();

		public async Task<ManagedIDisposableKey> RegisterIDisposable(IDisposable target)
		{
			ValidateDisposed();
			ManagedIDisposableKey key = new ManagedIDisposableKey { Id = Guid.NewGuid() };
			await RegisterIDisposable(target, key);
			return key;
		}

		public async Task RegisterIDisposable(IDisposable target, ManagedIDisposableKey key)
		{
			ValidateDisposed();
			int attempt = 0;
			while (!_items.TryAdd(key, target))
			{
				await Task.Delay(1);
				attempt++;
				if (attempt >= retryLimit)
				{
					throw new InvalidOperationException("Unable to manage resource - " + (target?.GetType()?.FullName ?? "unknown IDisposable"));
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing || Disposed)
			{
				base.Dispose(disposing);
				return;
			}
			if (_items.NotAnySafe())
			{
				base.Dispose(disposing);
				return;
			}
			while (_items.Any())
			{
				KeyValuePair<ManagedIDisposableKey, IDisposable> holder = _items.FirstOrDefault();
				ManagedIDisposableKey key = holder.Key;
				if (key.Id == Guid.Empty)
				{
					continue;
				}
				// _items[holder.Key] = null;
				int attempt = 0;
				IDisposable value;
				while (!_items.TryRemove(holder.Key, out value))
				{
					Thread.Sleep(1);
					attempt++;
					if (attempt >= retryLimit)
					{
						throw new InvalidOperationException("Unable to free resource - " + (value?.GetType()?.FullName ?? "unknown IDisposable"));
					}
				};
				key?.TriggerCallbefore(value, this);
				try
				{
					value?.Dispose();
				}
				catch (ObjectDisposedException)
				{
					// If it's already disposed that is ok
				}
				key?.TriggerCallback(this);
			}
			base.Dispose(disposing);
		}
	}
}