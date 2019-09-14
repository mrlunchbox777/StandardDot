using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Dto;
using StandardDot.TestClasses;

namespace StandardDot.Caching.Redis.UnitTests.Configuration
{
	public class TestSerializationSettings : ISerializationSettings
	{
		public TestSerializationSettings()
		{}

		public TestSerializationSettings(IEnumerable<Type> types)
		{
			_knownTypes.AddRange(types);
		}

		protected List<Type> _knownTypes => new List<Type>
		{
			typeof(Foo),
			typeof(Bar),
			typeof(Foobar),
			typeof(BarredFoo),
			typeof(RedisCachedObject<object>)
		};

		public List<Type> KnownTypes => _knownTypes;

		public DataContractResolver Resolver => null;
	}
}