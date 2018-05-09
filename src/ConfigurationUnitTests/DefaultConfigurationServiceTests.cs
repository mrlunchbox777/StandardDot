using System;
using StandardDot.Abstract.CoreServices.Serialization;
using StandardDot.Caching;
using StandardDot.Configuration;
using StandardDot.TestClasses.TestConfigurationMetadatas;
using StandardDot.TestClasses.TestConfigurations;
using Xunit;
using Xunit.Sdk;

namespace StandardDot.Configuration.UnitTests
{
    public class DefaultConfigurationServiceTests
    {
        [Fact]
        public void ProvidingMetadata()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfigurationMetadata metadata = new TestConfigurationMetadata(); 
            TestConfiguration configuration = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>(metadata);
            
            CheckConfiguration(configuration);
        }

        [Fact]
        // TODO: Something weird is going on here and it needs to be fixed (the stream isn't ever showing up as disposed)
        public void ProvidingMetadataStream()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfigurationMetadataStream metadata = new TestConfigurationMetadataStream();
            TestConfigurationStream configuration =
                service.GetConfiguration<TestConfigurationStream, TestConfigurationMetadataStream>(metadata);
            Assert.True(metadata.UsedAtLeastOnce);
            CheckConfigurationStream(configuration);

            service.ResetCachedConfigurations();
            TestConfigurationStream configuration2 =
                service.GetConfiguration<TestConfigurationStream, TestConfigurationMetadataStream>(metadata);
            CheckConfigurationStream(configuration, configuration2);
            Assert.True(metadata.UsedAtLeastOnce);
            Assert.True(metadata.StreamGotDisposed);
        }

        [Fact]
        public void ReflectingMetadata()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfiguration configuration = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            
            CheckConfiguration(configuration);
        }

        [Fact]
        public void ReflectingMetadataStream()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfigurationStream configuration =
                service.GetConfiguration<TestConfigurationStream, TestConfigurationMetadataStream>();

            CheckConfigurationStream(configuration);
        }

        [Fact]
        public void Caching()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfiguration configuration1 = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            TestConfiguration configuration2 = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            
            CheckConfiguration(configuration1, configuration2);
        }

        [Fact]
        public void CachingStream()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfigurationStream configuration1 = 
                service.GetConfiguration<TestConfigurationStream, TestConfigurationMetadataStream>();
            TestConfigurationStream configuration2 = 
                service.GetConfiguration<TestConfigurationStream, TestConfigurationMetadataStream>();
            
            CheckConfigurationStream(configuration1, configuration2);
        }

        [Fact]
        public void CompleteCacheClearing()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfiguration configuration1 = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            service.ResetCachedConfigurations();
            TestConfiguration configuration2 = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            
            CheckConfiguration(configuration1);
            CheckConfiguration(configuration2);
            Assert.Throws<EqualException>(() => Assert.Equal(configuration1, configuration2));
        }

        [Fact]
        public void CompleteCacheClearingStream()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfigurationStream configuration1 = 
                service.GetConfiguration<TestConfigurationStream, TestConfigurationMetadataStream>();
            service.ResetCachedConfigurations();
            TestConfigurationStream configuration2 = 
                service.GetConfiguration<TestConfigurationStream, TestConfigurationMetadataStream>();
            
            CheckConfigurationStream(configuration1);
            CheckConfigurationStream(configuration2);
            Assert.Throws<EqualException>(() => Assert.Equal(configuration1, configuration2));
        }

        [Fact]
        public void SpecificCacheClearing()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfiguration configuration1 = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            service.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>();
            TestConfiguration configuration2 = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            
            CheckConfiguration(configuration1);
            CheckConfiguration(configuration2);
            Assert.Throws<EqualException>(() => Assert.Equal(configuration1, configuration2));
        }

        [Fact]
        public void SpecificCacheClearingStream()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfigurationStream configuration1 = 
                service.GetConfiguration<TestConfigurationStream, TestConfigurationMetadataStream>();
            service.ClearConfiguration<TestConfigurationStream, TestConfigurationMetadataStream>();
            TestConfigurationStream configuration2 = 
                service.GetConfiguration<TestConfigurationStream, TestConfigurationMetadataStream>();
            
            CheckConfigurationStream(configuration1);
            CheckConfigurationStream(configuration2);
            Assert.Throws<EqualException>(() => Assert.Equal(configuration1, configuration2));
        }

        [Fact]
        public void CompleteCacheClearingMultiple()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfiguration configuration1 = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            TestConfiguration2 configurationt1 = 
                service.GetConfiguration<TestConfiguration2, TestConfigurationMetadata2>();
            service.ResetCachedConfigurations();
            TestConfiguration configuration2 = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            TestConfiguration2 configurationt2 = 
                service.GetConfiguration<TestConfiguration2, TestConfigurationMetadata2>();
            
            CheckConfiguration(configuration1);
            CheckConfiguration(configuration2);
            Assert.Throws<EqualException>(() => Assert.Equal(configuration1, configuration2));
            CheckConfiguration2(configurationt1);
            CheckConfiguration2(configurationt2);
            Assert.Throws<EqualException>(() => Assert.Equal(configurationt1, configurationt2));
        }

        [Fact]
        public void SpecificCacheClearingMultiple()
        {
            DefaultConfigurationService service = GenerateService();

            TestConfiguration configuration1 = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            TestConfiguration2 configurationt1 = 
                service.GetConfiguration<TestConfiguration2, TestConfigurationMetadata2>();
            service.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>();
            TestConfiguration configuration2 = 
                service.GetConfiguration<TestConfiguration, TestConfigurationMetadata>();
            TestConfiguration2 configurationt2 = 
                service.GetConfiguration<TestConfiguration2, TestConfigurationMetadata2>();
            
            CheckConfiguration(configuration1);
            CheckConfiguration(configuration2);
            Assert.Throws<EqualException>(() => Assert.Equal(configuration1, configuration2));
            CheckConfiguration2(configurationt1, configurationt2);
        }

        private static DefaultConfigurationService GenerateService()
        {
            Json jsonSerializer = new Json();
            MemoryCachingService cachingService = new MemoryCachingService(TimeSpan.FromDays(1), false);
            DefaultConfigurationCache cache = new DefaultConfigurationCache(cachingService,
                jsonSerializer, TimeSpan.FromDays(1));
            DefaultConfigurationService service = new DefaultConfigurationService(cache);
            return service;
        }

        private static void CheckConfiguration(TestConfiguration configuration)
        {
            Assert.NotNull(configuration);
            Assert.Equal(3, configuration.TestInt);
            Assert.NotEmpty(configuration.TestListString);
            Assert.Equal(3, configuration.TestListString.Count);
            Assert.Contains("string1", configuration.TestListString);
            Assert.Contains("string2", configuration.TestListString);
            Assert.Contains("string3", configuration.TestListString);
        }

        private static void CheckConfiguration(TestConfiguration configuration1, TestConfiguration configuration2)
        {
            CheckConfiguration(configuration1);
            Assert.Equal(configuration1, configuration2);
        }

        private static void CheckConfiguration2(TestConfiguration2 configuration)
        {
            Assert.NotNull(configuration);
            Assert.Equal(2, configuration.TestInt);
            Assert.NotEmpty(configuration.TestListString);
            Assert.Equal(4, configuration.TestListString.Count);
            Assert.Contains("string1", configuration.TestListString);
            Assert.Contains("string2", configuration.TestListString);
            Assert.Contains("string3", configuration.TestListString);
            Assert.Contains("string4", configuration.TestListString);
        }

        private static void CheckConfiguration2(TestConfiguration2 configuration1, TestConfiguration2 configuration2)
        {
            CheckConfiguration2(configuration1);
            Assert.Equal(configuration1, configuration2);
        }

        private static void CheckConfigurationStream(TestConfigurationStream configuration)
        {
            Assert.NotNull(configuration);
            TestConfiguration configurationProxy = new TestConfiguration
            {
                TestInt = configuration.TestInt,
                TestListString = configuration.TestListString
            };
            CheckConfiguration(configurationProxy);
        }

        private static void CheckConfigurationStream(TestConfigurationStream configuration1, TestConfigurationStream configuration2)
        {
            CheckConfigurationStream(configuration1);
            Assert.Equal(configuration1, configuration2);
        }
    }
}
