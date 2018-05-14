using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurationMetadatas;

namespace StandardDot.TestClasses.TestConfigurations
{
    [DataContract]
    public class TestConfigurationStream : ConfigurationBase<TestConfigurationStream, TestConfigurationMetadataStream>
    {
        [DataMember(Name = "testInt")]
        public int TestInt { get; set; }

        [DataMember(Name = "testListString")]
        public List<string> TestListString { get; set; }
    }
}