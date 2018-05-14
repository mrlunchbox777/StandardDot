using System;
using System.IO;
using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurations;

namespace StandardDot.TestClasses.TestConfigurationMetadatas
{
    public class TestConfigurationConstructorMetadata : ConfigurationMetadataBase<TestConfigurationConstructor, TestConfigurationConstructorMetadata>
    {
        public TestConfigurationConstructorMetadata()
        { }

        public TestConfigurationConstructorMetadata(string configurationLocation)
            : base(configurationLocation)
        { }

        public TestConfigurationConstructorMetadata(Func<Stream> getConfigurationStream)
            : base(getConfigurationStream)
        { }

        public override string ConfigurationName => "TestConfigurationConstructor";
    }
}