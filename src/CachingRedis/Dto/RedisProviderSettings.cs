using System;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Dto
{
	public class RedisProviderSettings : ICacheProviderSettings
	{
		public RedisProviderSettings(ISerializationService serializationService, ICacheServiceSettings serializableSettings
			, ConfigurationOptions configurationOptions, IDataContractResolver dataContractResolver = null)
		{
			SerializationService = serializationService;
			ServiceSettings = serializableSettings;
			DataContractResolver = dataContractResolver;
			ConfigurationOptions = configurationOptions == null
				? ConfigurationOptions.Parse(ServiceSettings.ConfigurationOptions, true)
				: configurationOptions;
		}

		public RedisProviderSettings(ISerializationService serializationService, string configurationOptionsString
			, ICacheServiceSettings serializableSettings, IDataContractResolver dataContractResolver = null)
			: this(serializationService, serializableSettings, ConfigurationOptions.Parse(configurationOptionsString, true)
				, dataContractResolver)
		{ }

		public RedisProviderSettings(ISerializationService serializationService, ICacheServiceSettings serializableSettings
			, IDataContractResolver dataContractResolver = null)
			: this(serializationService, serializableSettings, null, dataContractResolver)
		{ }

		public ConfigurationOptions ConfigurationOptions { get; set; }

		public ISerializationService SerializationService { get; set; }

		public IDataContractResolver DataContractResolver { get; set; }

		public ICacheServiceSettings ServiceSettings { get; set; }
	}
}
