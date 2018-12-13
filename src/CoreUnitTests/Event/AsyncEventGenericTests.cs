using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StandardDot.Core.Event;
using Xunit;

namespace StandardDot.Core.UnitTests.Event
{
	public class AsyncEventGenericTests
	{
		[Fact]
		public async Task LoggingActionTest()
		{
			int threw = 0;
			List<Exception> logged = new List<Exception>();
			Func<object, EventArgs, Task> thrower = async (x, y) =>
				await Task.Run(() => { threw ++; throw new InvalidOperationException(); });
			Func<Exception, Task> logger = async (ex) => await Task.Run(() => { logged.Add(ex); });
			AsyncEvent<object, EventArgs> myEvent
				= new AsyncEvent<object, EventArgs>(logger);
			myEvent += thrower;

			Exception exception = null;
			bool didThrow = false;
			try
			{
				await myEvent.Raise(this, null);
			}
			catch (Exception ex)
			{
				didThrow = true;
				Assert.Equal(logged.First(), ex);
				exception = ex;
			}

			Assert.True(didThrow);
			Assert.Equal(1, threw);
			Assert.Equal(threw, logged.Count);
			foreach(var log in logged)
			{
				Assert.Equal(exception, log);
			}
		}
	}
}
