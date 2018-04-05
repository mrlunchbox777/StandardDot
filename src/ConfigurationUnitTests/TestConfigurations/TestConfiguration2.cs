using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using StandardDot.Abstract.Configuration;
using StandardDot.ConfigurationUnitTests.TestConfigurationMetadatas;

namespace StandardDot.ConfigurationUnitTests.TestConfigurations
{
    [DataContract]
    public class TestConfiguration2 : ConfigurationBase<TestConfiguration2, TestConfigurationMetadata2>
    {
        [DataMember(Name = "testInt")]
        public int TestInt { get; set; }

        [DataMember(Name = "testListString")]
        public List<string> TestListString { get; set; }
    }
}