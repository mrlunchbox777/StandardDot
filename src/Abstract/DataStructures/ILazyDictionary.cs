using System.Collections.Generic;

namespace StandardDot.Abstract.DataStructures
{
	public interface ILazyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
	{
		/// <summary>
		/// Converts the lazy dictionary to an actual dictionary.
		/// This operation may be expensive, use with caution
		/// </summary>
		/// <returns>The enumerated dictionary</returns>
		IDictionary<TKey, TValue> EnumerateDictionary();

		/// <summary>
		/// These keys have been modified with <c>new</c> to use the ILazyCollection,
		/// however, that inherits from ICollection so it should be
		/// able to be used without issue
		/// </summary>
		new ILazyCollection<TKey> Keys { get; }

		/// <summary>
		/// These values have been modified with <c>new</c> to use the ILazyCollection,
		/// however, that inherits from ICollection so it should be
		/// able to be used without issue
		/// </summary>
		new ILazyCollection<TValue> Values { get; }
	}
}