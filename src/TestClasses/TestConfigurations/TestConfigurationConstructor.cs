using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurationMetadatas;

namespace StandardDot.TestClasses.TestConfigurations
{
    [DataContract]
    public class TestConfigurationConstructor : ConfigurationBase<TestConfigurationConstructor, TestConfigurationConstructorMetadata>
    {
        public TestConfigurationConstructor()
            : base()
        {}

        public TestConfigurationConstructor(TestConfigurationConstructorMetadata metadata)
            : base(metadata)
        {}

        [DataMember(Name = "testInt")]
        public int TestInt { get; set; }

        [DataMember(Name = "testListString")]
        public List<string> TestListString { get; set; }
    }
}