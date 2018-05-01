using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using StandardDot.Abstract;

namespace StandardDot.Authentication.Hmac
{
    /// <summary>
    /// An Api Key Service backed by a string, string dictionary
    /// </summary>
    public class BasicApiKeyService : IApiKeyService
    {
        /// <param name="source">The backing dictionary that provides api keys and secrets</param>
        public BasicApiKeyService(IDictionary<string, string> source)
        {
            Source = source;
        }

        protected virtual IDictionary<string, string> Source { get; set; }

        public string GenerateApiKey()
        {
            using (RNGCryptoServiceProvider cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] secretKeyByteArray = new byte[32]; //256 bit
                cryptoProvider.GetBytes(secretKeyByteArray);
                string apiKey = Convert.ToBase64String(secretKeyByteArray);
                return apiKey;
            }
        }

        public virtual string this[string key] { get => Source[key]; set => Source[key] = value; }

        public virtual ICollection<string> Keys => Source.Keys;

        public virtual ICollection<string> Values => Source.Values;

        public virtual int Count => Source.Count;

        public virtual bool IsReadOnly => Source.IsReadOnly;

        public virtual void Add(string key, string value)
        {
            Source.Add(key, value);
        }

        public virtual void Add(KeyValuePair<string, string> item)
        {
            Source.Add(item);
        }

        public virtual void Clear()
        {
            Source.Clear();
        }

        public virtual bool Contains(KeyValuePair<string, string> item)
        {
            return Source.Contains(item);
        }

        public virtual bool ContainsKey(string key)
        {
            return Source.ContainsKey(key); 
        }

        public virtual void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            Source.CopyTo(array, arrayIndex); 
        }

        public virtual IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Source.GetEnumerator(); 
        }

        public virtual bool Remove(string key)
        {
            return Source.Remove(key); 
        }

        public virtual bool Remove(KeyValuePair<string, string> item)
        {
            return Source.Remove(item);
        }

        public virtual bool TryGetValue(string key, out string value)
        {
            return Source.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Source).GetEnumerator();
        }
    }
}