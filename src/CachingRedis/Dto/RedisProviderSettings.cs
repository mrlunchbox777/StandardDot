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
			, ISerializationSettings serializationSettings = null)
			: this(serializationService, serializableSettings, null, serializationSettings)
		{ }

		public RedisProviderSettings(ISerializationService serializationService, ICacheServiceSettings serializableSettings
			, ConfigurationOptions configurationOptions, ISerializationSettings serializationSettings = null)
		{
			SerializationService = serializationService;
			ServiceSettings = serializableSettings;
			SerializationSettings = serializationSettings;
			ConfigurationOptions = configurationOptions == null
				? ConfigurationOptions.Parse(ServiceSettings.ConfigurationOptions, true)
				: configurationOptions;
		}

		public RedisProviderSettings(ISerializationService serializationService, string configurationOptionsString
			, ICacheServiceSettings serializableSettings, ISerializationSettings serializationSettings = null)
			: this(serializationService, serializableSettings, ConfigurationOptions.Parse(configurationOptionsString, true)
				, serializationSettings)
		{ }

		public virtual ConfigurationOptions ConfigurationOptions { get; set; }

		public virtual ISerializationService SerializationService { get; set; }

		public virtual ISerializationSettings SerializationSettings { get; set; }

		public virtual ICacheServiceSettings ServiceSettings { get; set; }
	}
}
