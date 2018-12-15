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

			bool didThrow = false;
			int aggregateCount = 0;
			try
			{
				await myEvent.Raise(this, null);
			}
			catch (Exception ex)
			{
				didThrow = true;
				var notAggregate = ex;
				while (notAggregate is AggregateException)
				{
					aggregateCount++;
					notAggregate = notAggregate.InnerException;
				}
				Assert.True(notAggregate is InvalidOperationException);
				Assert.Equal(logged.Single(), notAggregate);
				Assert.True(ex is AggregateException);
				// we don't log the aggregate
				// Assert.Equal(logged.Last(), ex);
			}

			Assert.True(didThrow);
			Assert.Equal(1, aggregateCount);
			Assert.Equal(1, logged.Count);
			Assert.Equal(threw, logged.Count);
		}
	}
}
