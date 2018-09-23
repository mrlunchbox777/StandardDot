using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace StandardDot.CoreExtensions
{
	/// <summary>
	/// Extensions for List.
	/// </summary>
	public static class ListExtensions
	{
		/// <summary>
		/// Keeps the order of all other items in the list, but moves one item to the front
		/// </summary>
		/// <typeparam name="T">Item type</typeparam>
		/// <param name="list">Target list</param>
		/// <param name="itemSelector">Predicate to find the item to move to the front (finds the first match)</param>
		public static void MoveItemToFirst<T>(this List<T> list, Predicate<T> itemSelector)
		{
			int index = list.FindIndex(itemSelector);
			T item = list[index];
			list.RemoveAt(index);
			list.Insert(0, item);
		}
	}
}