using System;
using StandardDot.CoreServices.Manager;
using Xunit;

namespace StandardDot.CoreServices.UnitTests.Manager
{
	public class ManagedIDisposableKeyTests
	{
		[Fact]
		public void PropertyTests()
		{
			ManagedIDisposableKey managedIDisposableKey = new ManagedIDisposableKey();
			Guid id = Guid.Empty;
			// managedIDisposableKey.Id = id;

			Assert.Equal(id, managedIDisposableKey.Id);
			Assert.True(managedIDisposableKey.Equals(id));
			Assert.Equal(id.GetHashCode(), managedIDisposableKey.GetHashCode());
			Assert.Equal(id.ToString(), managedIDisposableKey.ToString());
			Assert.Equal(id.ToString("n"), managedIDisposableKey.ToString("n"));

			id = Guid.NewGuid();
			managedIDisposableKey.Id = id;
			Assert.Equal(id, managedIDisposableKey.Id);
			Assert.True(managedIDisposableKey.Equals(id));
			Assert.Equal(id.GetHashCode(), managedIDisposableKey.GetHashCode());
			Assert.Equal(id.ToString(), managedIDisposableKey.ToString());
			Assert.Equal(id.ToString("n"), managedIDisposableKey.ToString("n"));
		}

		[Fact]
		public void EqualityTests()
		{
			ManagedIDisposableKey managedIDisposableKey = new ManagedIDisposableKey();
			ManagedIDisposableKey managedIDisposableKey2 = new ManagedIDisposableKey();
			ManagedIDisposableKey managedIDisposableKey3 = new ManagedIDisposableKey();
			Guid id = Guid.NewGuid();
			Guid id2 = Guid.NewGuid();
			managedIDisposableKey.Id = id;
			managedIDisposableKey2.Id = id;
			managedIDisposableKey3.Id = id2;

			Assert.Equal(managedIDisposableKey, managedIDisposableKey2);
			Assert.NotEqual(managedIDisposableKey, managedIDisposableKey3);
			Assert.NotEqual(managedIDisposableKey2, managedIDisposableKey3);

			Assert.True(managedIDisposableKey == managedIDisposableKey2);
			Assert.False(managedIDisposableKey == managedIDisposableKey3);
			Assert.False(managedIDisposableKey2 == managedIDisposableKey3);

			Assert.False(managedIDisposableKey != managedIDisposableKey2);
			Assert.True(managedIDisposableKey != managedIDisposableKey3);
			Assert.True(managedIDisposableKey2 != managedIDisposableKey3);
		}

		// [Fact]
		// public void EventTests
	}
}