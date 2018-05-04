using System;
using Moq;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.Configuration;
using Xunit;

namespace Abstract.UnitTests.Configuration
{
    public class MoqObjects
    {
        public Mock<IConfiguration, IConfigurationMetadata> GetMetaData()
        {
            Mock<IConfigurationMetaData> metaData = new Mock<IConfigurationMetaData>(MockBehavior.Strict);
        }
    }
}