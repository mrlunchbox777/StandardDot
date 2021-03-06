using System.Collections;
using System.Collections.Generic;
using StandardDot.Abstract;

namespace StandardDot.Abstract.IntegrationTests.AbstractObjects
{
	public class TestApiKeyService : IApiKeyService
	{
		public TestApiKeyService()
		{
			Source = new Dictionary<string, string>();
		}

		protected virtual IDictionary<string, string> Source { get; set; }

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