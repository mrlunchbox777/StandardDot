using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Moq.Protected;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;
using Xunit;

namespace StandardDot.Abstract.UnitTests.CoreServices
{
    public class LogEnumerableBaseTests
    {
        [Fact]
        public void ConstructorTests()
        {
            IEnumerable<Log<object>> source = new List<Log<object>> { new Log<object>() };
            Mock<LogEnumerableBase<object>> enumerable = new Mock<LogEnumerableBase<object>>(MockBehavior.Loose, source);
            enumerable.CallBase = true;

            Assert.Single(enumerable.Object);
            Assert.Equal(source.Single(), enumerable.Object.Single());

            Mock<LogEnumerableBase<object>> enumerable2 = new Mock<LogEnumerableBase<object>>(MockBehavior.Loose, enumerable.Object);
            enumerable2.CallBase = true;

            Assert.Single(enumerable2.Object);
            Assert.Equal(source.Single(), enumerable2.Object.Single());
        }

        [Fact]
        public void GetGenericEnumeratorTest()
        {
            IEnumerable<Log<object>> source = new List<Log<object>> { new Log<object>() };
            Mock<LogEnumerableBase<object>> enumerable = new Mock<LogEnumerableBase<object>>(MockBehavior.Loose, source);
            enumerable.CallBase = true;

            using(IEnumerator<Log<object>> enumerator = enumerable.Object.GetEnumerator()) {
                Assert.Null(enumerator.Current);
                Assert.True(enumerator.MoveNext());
                Assert.Equal(source.Single(), enumerator.Current);
                Assert.False(enumerator.MoveNext());
                Assert.Null(enumerator.Current);
            }
        }

        [Fact]
        public void GetEnumeratorTest()
        {
            IEnumerable<Log<object>> source = new List<Log<object>> { new Log<object>() };
            Mock<LogEnumerableBase<object>> enumerable = new Mock<LogEnumerableBase<object>>(MockBehavior.Loose, source);
            enumerable.CallBase = true;

            IEnumerator enumerator = ((IEnumerable)enumerable.Object).GetEnumerator();
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(source.Single(), enumerator.Current);
            Assert.False(enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        }
    }
}