using System.Linq;
using System;
using System.Collections.Generic;

namespace StandardDot.CoreExtensions
{
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Returns an Enumerable that is distinct by the function given
		/// </summary>
		/// <typeparam name="TSource">The type in the source <see cref="IEnumerable" /></typeparam>
		/// <typeparam name="TKey">They type of the hash for each element</typeparam>
		/// <param name="source">The source IEnumerable</param>
		/// <param name="keySelector">The function that returns a unique hash for each element</param>
		/// <returns>The filtered IEnumerable</returns>
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>
			(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> knownKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (knownKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}
		
		/// <summary>
		/// Returns an Enumerable that is distinct by the <see cref="GetHashCode" />
		/// </summary>
		/// <typeparam name="TSource">The type in the source <see cref="IEnumerable" /></typeparam>
		/// <param name="source">The source IEnumerable</param>
		/// <returns>The filtered IEnumerable</returns>
		public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
		{
			return DistinctBy(source, x => x.GetHashCode());
		}
		
		/// <summary>
		/// If there are any elements in a collection, default false
		/// <note> any AND !null </note>
		/// </summary>
		/// <typeparam name="TSource">The type in the source <see cref="IEnumerable" /></typeparam>
		/// <param name="source">The source IEnumerable</param>
		/// <returns>If there are any elements in a collection</returns>
		public static bool AnySafe<TSource>(this IEnumerable<TSource> source)
			=> source?.Any() ?? false;
		
		/// <summary>
		/// If there are not elements in a collection, default true
		/// <note> !any || null </note>
		/// </summary>
		/// <typeparam name="TSource">The type in the source <see cref="IEnumerable" /></typeparam>
		/// <param name="source">The source IEnumerable</param>
		/// <returns>If there are any elements in a collection</returns>
		public static bool NotAnySafe<TSource>(this IEnumerable<TSource> source)
			=> !source.AnySafe();
	}
}