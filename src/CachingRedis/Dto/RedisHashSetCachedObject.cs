using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Dto
{
    public class RedisHashSetCachedObject<T> : RedisCachedObject<T>
    {
        public RedisHashSetCachedObject(string hashsetIdentifier, string objectIdentifier)
            : base(objectIdentifier)
        {
            Id.HashSetIdentifier = hashsetIdentifier;
            Id.ServiceType = RedisServiceType.HashSet;
        }
    }
}
