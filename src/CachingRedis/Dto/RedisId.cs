using System;
using System.Linq;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Dto
{
	public class RedisId
	{
		protected internal RedisId(){}
		public RedisId(RedisServiceType serviceType, string objectIdentifier, string hashSetIdentifier = null)
		{
			ServiceType = serviceType;
			ObjectIdentifier = objectIdentifier;
			HashSetIdentifier = hashSetIdentifier;
		}
		private static readonly string KeySeperator = "---___+++";
		public string HashSetIdentifier { get; set; }

		public string ObjectIdentifier { get; set; }

		public RedisServiceType ServiceType { get; set; }

		public string FullKey
		{
			get
			{
				switch (ServiceType)
				{
					case RedisServiceType.HashSet:
						return (HashSetIdentifier ?? "") + ":" + (ObjectIdentifier ?? "");
					case RedisServiceType.KeyValue:
					default:
						return ObjectIdentifier ?? "";
				}
			}
		}

		public string CacheKey
		{
			get
			{
				string intro = ServiceType + KeySeperator;
				if (!HasFullKey)
				{
					return "";
				}
				switch (ServiceType)
				{
					case RedisServiceType.HashSet:
						return intro + (HashSetIdentifier ?? "") + KeySeperator + (ObjectIdentifier ?? "");
					case RedisServiceType.KeyValue:
					default:
						return intro + (ObjectIdentifier ?? "");
				}
			}
		}

		public static implicit operator string(RedisId id)
		{
			return id?.CacheKey;
		}

		public static implicit operator RedisId(string id)
		{
			RedisId newId = new RedisId();
			if (string.IsNullOrWhiteSpace(id))
			{
				return newId;
			}
			if (!id.Contains(KeySeperator))
			{
				newId.ObjectIdentifier = id;
				newId.ServiceType = RedisServiceType.KeyValue;
				return newId;
			}
			string[] parts = id.Split(new[] {KeySeperator}, StringSplitOptions.None);
			if (!(parts?.Any() ?? false))
			{
				newId.ObjectIdentifier = id;
				newId.ServiceType = RedisServiceType.KeyValue;
				return newId;
			}
			bool gotTheServiceType = Enum.TryParse(parts.First(), out RedisServiceType serviceType);
			if (!gotTheServiceType)
			{
				newId.ObjectIdentifier = id;
				newId.ServiceType = RedisServiceType.KeyValue;
				return newId;
			}
			newId.ServiceType = serviceType;
			switch (serviceType)
			{
				case RedisServiceType.HashSet:
					newId.HashSetIdentifier = parts.Skip(1).Take(1).SingleOrDefault();
					newId.ObjectIdentifier = parts.Skip(2).Aggregate("", (x, y) => x + y);
					break;
				case RedisServiceType.KeyValue:
					newId.ObjectIdentifier = parts.Skip(1).Aggregate("", (x, y) => x + y);
					break;
				case RedisServiceType.Provider:
				default:
					newId.ServiceType = RedisServiceType.KeyValue;
					goto case RedisServiceType.KeyValue;
			}
		}

		public bool HasFullKey
		{
			get
			{
				switch (ServiceType)
				{
					case RedisServiceType.HashSet:
						return !(string.IsNullOrWhiteSpace(HashSetIdentifier) || string.IsNullOrWhiteSpace(ObjectIdentifier));
					case RedisServiceType.KeyValue:
					default:
						return !(string.IsNullOrWhiteSpace(ObjectIdentifier));
				}
			}
		}
	}
}