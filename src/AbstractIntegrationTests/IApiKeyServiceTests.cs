using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StandardDot.Abstract;
using StandardDot.AbstractUnitTests.AbstractObjects;
using Xunit;

namespace StandardDot.Abstract.IntegrationTests
{
    public class IApiKeyServiceTests
    {
        [Fact]
        public void Keys()
        {
            IApiKeyService service = GetService();
            Assert.Single(service.Keys);
            Assert.Equal(_appId, service.Keys.Single());
        }

        [Fact]
        public void Values()
        {
            IApiKeyService service = GetService();
            Assert.Single(service.Values);
            Assert.NotNull(service.Values.Single());
            Assert.Equal(_secretKey, service.Values.Single());
        }

        [Fact]
        public void Count()
        {
            IApiKeyService service = GetService();
            Assert.Single(service);
        }

        [Fact]
        public void Indexing()
        {
            IApiKeyService service = GetService();

            service[_appId2] = _secretKey2;
            Assert.Equal(2, service.Count);
            string retrieved = service[_appId2];
            Assert.Equal(_secretKey2, retrieved);
        }

        [Fact]
        public void Add()
        {
            IApiKeyService service = GetService();

            service.Add(_appId2, _secretKey2);
            Assert.Equal(2, service.Count);
            Assert.Equal(_secretKey2, service[_appId2]);
        }

        [Fact]
        public void ContainsKey()
        {
            IApiKeyService service = GetService();

            Assert.True(service.ContainsKey(_appId));
        }

        [Fact]
        public void Remove()
        {
            IApiKeyService service = GetService();

            string retrievedSecret = service[_appId];

            service.Remove(_appId);
            string retrievedSecret2 = null;
            Assert.Throws<KeyNotFoundException>(() => retrievedSecret2 = service[_appId]);
            Assert.Empty(service);
            Assert.NotNull(retrievedSecret);
            Assert.Null(retrievedSecret2);
        }

        [Fact]
        public void TryGetValue()
        {
            IApiKeyService service = GetService();
            string result;
            Assert.False(service.TryGetValue(_appId2, out result));

            service.Add(_appId2, _secretKey2);
            bool success = service.TryGetValue(_appId2, out result);
            Assert.True(success);
            Assert.Equal(_secretKey2, result);
        }

        [Fact]
        public void AddKvp()
        {
            IApiKeyService service = GetService();
            KeyValuePair<string, string> item = new KeyValuePair<string, string>(_appId2, _secretKey2);

            service.Add(item);
            string result = service[_appId2];
            Assert.Equal(_secretKey2, result);
        }

        [Fact]
        public void Clear()
        {
            IApiKeyService service = GetService();

            Assert.Single(service);
            Assert.Equal(_secretKey, service[_appId]);

            service.Clear();
            Assert.Empty(service);
        }

        [Fact]
        public void Contains()
        {
            IApiKeyService service = GetService();
            KeyValuePair<string, string> item = new KeyValuePair<string, string>(_appId2, _secretKey2);

            Assert.DoesNotContain(item, service);
            service.Add(item);
            Assert.Contains(item, service);
        }

        [Fact]
        public void CopyTo()
        {
            IApiKeyService service = GetService();

            string retrieved = service[_appId];
            KeyValuePair<string, string>[] cache = new KeyValuePair<string, string>[1];
            service.CopyTo(cache, 0);
            Assert.NotNull(cache);
            Assert.NotEmpty(cache);
            Assert.Single(cache);
            Assert.Equal(retrieved, cache[0].Value);
            Assert.Equal(_appId, cache[0].Key);
        }

        [Fact]
        public void RemoveKvp()
        {
            IApiKeyService service = GetService();
            service.Clear();
            KeyValuePair<string, string> item = new KeyValuePair<string, string>(_appId2, _secretKey2);
            bool succeded = service.Remove(item);
            Assert.False(succeded);

            service.Add(item);
            string result = service[_appId2];
            Assert.Equal(_secretKey2, result);
            succeded = service.Remove(item);
            Assert.True(succeded);
            Assert.Empty(service);
        }

        [Fact]
        public void TestEnumerators()
        {
            IApiKeyService service = GetService();

            IEnumerator<KeyValuePair<string, string>> typedEnumerator
                = service.GetEnumerator();
            IEnumerator enumerator = ((IEnumerable)service).GetEnumerator();
            typedEnumerator.MoveNext();
            enumerator.MoveNext();
            Assert.Equal(_secretKey,
                (typedEnumerator.Current).Value);
            Assert.Equal(_secretKey,
                    ((KeyValuePair<string, string>)enumerator.Current).Value);
        }

        private static IApiKeyService GetService()
        {
            IApiKeyService backingKeys = new TestApiKeyService();
            backingKeys.Add(_appId, _secretKey);
            return backingKeys;
        }
        
        private const string _appId = "app1";
        
        private const string _secretKey = "secretKey1";

        private const string _appId2 = "app2";
        
        private const string _secretKey2 = "secretKey2";
    }
}