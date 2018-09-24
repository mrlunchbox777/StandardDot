using System;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface ICacheProviderSettings
	{
		ConfigurationOptions ConfigurationOptions { get; set; }

		ICacheServiceSettings ServiceSettings { get; set; }

		ISerializationService SerializationService { get; set; }

		/// <summary>The datacontract resolver to use for serialization (polymorphic dtos)</summary>
		ISerializationSettings SerializationSettings { get; set; }
	}
}