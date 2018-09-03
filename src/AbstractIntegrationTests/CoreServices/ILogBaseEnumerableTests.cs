using System;
using System.Collections.Generic;
using Moq;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;
using Xunit;

namespace StandardDot.Abstract.IntegrationTests.CoreServices
{
    public class ILogBaseEnumerableTests
    {
        [Fact]
        public void GenericTypeTest()
        {
            Mock<ILogBaseEnumerable> enumerableProxy = new Mock<ILogBaseEnumerable>(MockBehavior.Strict);
            Type proxyType = enumerableProxy.Object.GetType();
            Assert.True(typeof(ILogBaseEnumerable).IsAssignableFrom(proxyType));
            Assert.True(typeof(IEnumerable<LogBase>).IsAssignableFrom(proxyType));
        }
    }
}