using System;
using System.Collections.Generic;
using Moq;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;
using Xunit;

namespace AbstractUnitTests.CoreServices
{
    public class ILogEnumerableTests
    {
        [Fact]
        public void GenericTypeTest()
        {
            Mock<ILogEnumerable<object>> enumerableProxy = new Mock<ILogEnumerable<object>>(MockBehavior.Strict);
            Type proxyType = enumerableProxy.Object.GetType();
            Assert.True(typeof(ILogEnumerable<object>).IsAssignableFrom(proxyType));
            Assert.True(typeof(IEnumerable<Log<object>>).IsAssignableFrom(proxyType));
        }
    }
}