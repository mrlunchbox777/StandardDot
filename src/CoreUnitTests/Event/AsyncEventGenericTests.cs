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
				await Task.Run(() => { threw++; throw new InvalidOperationException(); });
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
			Assert.Single(logged);
			Assert.Equal(threw, logged.Count);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ExceptionHandlerTest(bool shouldThrow)
		{
			int threw = 0;
			List<Tuple<Exception, object, EventArgs>> logged = new List<Tuple<Exception, object, EventArgs>>();
			Func<object, EventArgs, Task> thrower = async (x, y) =>
				await Task.Run(() => { threw++; throw new InvalidOperationException(); });
			Func<Exception, object, EventArgs, Task<bool>> exceptionHandler = async (x, y, z) =>
				{
					await Task.Run(() => { logged.Add(new Tuple<Exception, object, EventArgs>(x, y, z)); });
					return shouldThrow;
				};

			AsyncEvent<object, EventArgs> myEvent
				= new AsyncEvent<object, EventArgs>();
			myEvent += thrower;

			bool didThrow = false;
			int aggregateCount = 0;
			try
			{
				await myEvent.Raise(this, null, exceptionHandler);
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
				Assert.True(ex is AggregateException);
			}

			int expectedCount = shouldThrow ? 1 : 0;

			Assert.Equal(shouldThrow, didThrow);
			Assert.Equal(expectedCount, aggregateCount);
			Assert.Equal(1, threw);
			Assert.Single(logged);
			Assert.True(logged[0].Item1 is InvalidOperationException);
			Assert.True(logged[0].Item2 is AsyncEventGenericTests);
			Assert.Null(logged[0].Item3);
		}
	}
}
