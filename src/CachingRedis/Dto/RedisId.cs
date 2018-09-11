using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Dto
{
    public class RedisId
    {
        public string HashSetIdentifier { get; set; }

        public string ObjectIdentifier { get; set; }

        public RedisServiceType ServiceType { get; set; }

        public string FullKey
        {
            get
            {
                switch(ServiceType)
                {
                    case RedisServiceType.HashSet:
                        return (HashSetIdentifier ?? "") + ":" + (ObjectIdentifier ?? "");
                    case RedisServiceType.KeyValue:
                    default:
                        return ObjectIdentifier ?? "";
                }
            }
        }

        public bool HasFullKey
        {
            get
            {
                switch(ServiceType)
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