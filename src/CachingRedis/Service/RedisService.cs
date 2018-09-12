using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;
using StandardDot.Caching.Redis.Providers;

namespace StandardDot.Caching.Redis.Service
{
    internal class RedisService
    {
        public RedisService(ICacheProviderSettings settings)
        {
            _cacheSettings = settings;
        }

        private ICacheProviderSettings _cacheSettings;

        /// <summary>
        /// Set this before using anything
        /// </summary>
        public ICacheProviderSettings CacheSettings => _cacheSettings;

        private static RedisCacheProvider _cacheProvider;

        internal RedisCacheProvider CacheProvider
        {
            get
            {
                if (_cacheProvider == null)
                {
                    ResetCache();
                }
                return _cacheProvider;
            }
            private set { _cacheProvider = value; }
        }

        private static ConnectionMultiplexer _redis;

        private ConnectionMultiplexer Redis
        {
            get
            {
                if (_redis != null && _redis.IsConnected)
                {
                    return _redis;
                }

                ResetCache();
                _redis = CacheProvider?.GetRedis();
                return _redis;
            }
        }

        private static IDatabase _db;

        internal IDatabase Database
        {
            get
            {
                _db = Redis?.GetDatabase();
                return _db;
            }
        }

        private static IServer _server;

        internal IServer Server(string key)
        {
            _server = Redis?.GetServer(Database.IdentifyEndpoint(key));
            return _server;
        }

        private static readonly ConcurrentDictionary<RedisServiceType, IRedisService> _redisServiceImplementation =
            new ConcurrentDictionary<RedisServiceType, IRedisService>();

        /// <summary>
        /// If you are going to set this, make sure that you change the redisserviceimplementationtype first
        /// </summary>
        public IRedisService RedisServiceImplementation
        {
            get
            {
                return GetEnsureValidRedisServiceImplementation(CacheSettings.RedisServiceImplementationType);
            }
            set
            {

                if (_redisServiceImplementation.ContainsKey(CacheSettings.RedisServiceImplementationType))
                {
                    _redisServiceImplementation[CacheSettings.RedisServiceImplementationType] = value;
                }
                _redisServiceImplementation.TryAdd(CacheSettings.RedisServiceImplementationType, value);
            }
        }

        private IRedisService GetEnsureValidRedisServiceImplementation(
            RedisServiceType redisServiceImplementationType)
        {
            if (_redisServiceImplementation.ContainsKey(CacheSettings.RedisServiceImplementationType)
                && (_redisServiceImplementation[CacheSettings.RedisServiceImplementationType] != null))
            {
                return _redisServiceImplementation[CacheSettings.RedisServiceImplementationType];
            }

            IRedisService helper;
            switch (redisServiceImplementationType)
            {
                case RedisServiceType.HashSet:
                    helper = new HashSetRedisService(this);
                    break;
                case RedisServiceType.KeyValue:
                    helper = new BasicRedisService(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(redisServiceImplementationType));
            }

            if (_redisServiceImplementation.ContainsKey(redisServiceImplementationType))
            {
                _redisServiceImplementation[redisServiceImplementationType] = helper;
            }
            else
            {
                _redisServiceImplementation.TryAdd(redisServiceImplementationType, helper);
            }
            return _redisServiceImplementation[redisServiceImplementationType];
        }

        private IRedisService GetRedisServiceImplementation(RedisServiceType? type = null)
        {
            if (type == null)
            {
                return RedisServiceImplementation;
            }

            return GetEnsureValidRedisServiceImplementation((RedisServiceType)type);
        }


        private void ResetCache(RedisCacheProvider provider = null, bool forceReset = false)
        {
            if (provider == null || forceReset)
            {
                CacheProvider = new RedisCacheProvider(this.CacheSettings);
            }
            else
            {
                CacheProvider = provider;
            }
        }

        /// <summary>
        /// Converts a string found in cache to an object
        /// </summary>
        /// <typeparam name="T">Object type to convert to</typeparam>
        /// <param name="stringToConvert">String to convert to an object</param>
        /// <param name="key">The key that was used to get the value</param>
        /// <param name="dataContractResolver">The datacontract resolver to use for serialization
        /// (polymorphic dtos)</param>
        /// <returns>Object from string</returns>
        internal T ConvertString<T>(string stringToConvert, RedisId key,
            IDataContractResolver dataContractResolver = null)
        {
            T retVal = default(T);

            if (string.IsNullOrWhiteSpace(stringToConvert))
            {
                return retVal;
            }

            if (typeof(T) == typeof(string))
            {
                retVal = (T)Convert.ChangeType(stringToConvert, typeof(T));
            }
            else
            {
                ISerializationService serializationService = GetSerializationService<T>(dataContractResolver); 
                retVal = serializationService.DeserializeObject<T>(stringToConvert);
            }

            return retVal;
        }

        internal ISerializationService GetSerializationService<T>(IDataContractResolver dataContractResolver)
        {
            return CacheSettings.SerializationService;
        }
    }
}