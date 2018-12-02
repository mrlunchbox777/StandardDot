using System.Linq;
using System.Runtime.Serialization;
using StandardDot.Caching.Redis.Enums;
using StandardDot.CoreExtensions;
using StandardDot.CoreExtensions.Object;

namespace StandardDot.Caching.Redis.Dto
{
	[DataContract]
	public class RedisId
	{
		// new []
		// {
		// 	"hashSetIdentifier",
		// 	"objectIdentifier",
		// 	"serviceType"
		// };
		private static string[] _dataMemberNames =
			typeof(RedisId).GetProperties()
				.Select(x => x.GetCustomAttributes(typeof(DataMemberAttribute), true).Cast<DataMemberAttribute>())
				.SelectMany(x => x).Select(x => x.Name)
				.ToArray();

		public static string[] DataMemberNames => _dataMemberNames;

		[DataMember(Name = "hashSetIdentifier")]
		public string HashSetIdentifier { get; set; }

		[DataMember(Name = "objectIdentifier")]
		public string ObjectIdentifier { get; set; }

		[DataMember(Name = "serviceType")]
		public RedisServiceType ServiceType { get; set; }

		[IgnoreDataMember]
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
						return (string.IsNullOrWhiteSpace(HashSetIdentifier) ? "" : HashSetIdentifier + ":")
							+ (ObjectIdentifier ?? "");
				}
			}
		}

		[IgnoreDataMember]
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

		public static implicit operator string(RedisId id)
		{
			return id?.SerializeJson();
		}

		public static implicit operator RedisId(string id)
		{
			return id?.DeserializeJson<RedisId>();
		}
	}
}