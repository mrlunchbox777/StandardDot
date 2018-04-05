using System;
using System.IO;
using StandardDot.Abstract.Configuration;
using StandardDot.ConfigurationUnitTests.TestConfigurations;

namespace StandardDot.ConfigurationUnitTests.TestConfigurationMetadatas
{
    public class TestConfigurationMetadataStream : ConfigurationMetadataBase<TestConfigurationStream, TestConfigurationMetadataStream>
    {
        public TestConfigurationMetadataStream()
        {
            EnsureStream();
        }

        private void EnsureStream()
        {
            _currentStream = File.OpenRead("./testConfigurationJson.json");
        }

        private Stream _currentStream { get; set; }

        public bool StreamGotDisposed { get; set; }

        public bool UsedAtLeastOnce { get; set; }

        public override Func<Stream> GetConfigurationStream 
        {
            get
            {
                return (() =>
                {
                    if (UsedAtLeastOnce && _currentStream == null)
                    {
                        StreamGotDisposed = true;
                    }
                    else if (UsedAtLeastOnce)
                    {
                        throw new InvalidOperationException("this is weird");
                    }
                    EnsureStream();
                    UsedAtLeastOnce = true;
                    return _currentStream;
                });
            }
        }

        public override bool UseStream => true;

        public override string ConfigurationName => "TestConfigurationStream";
    }
}