using System;
using System.Collections;
using System.Collections.Generic;

namespace StandardDot.Abstract.DataStructures
{
	public abstract class BaseLazyCollection<T> : ILazyCollection<T>
	{
		public BaseLazyCollection(IEnumerable<T> source)
		{
			_source = source;
			_enumeratedSoFar = new List<T>();
		}

		protected IEnumerable<T> _source;

		protected List<T> _enumeratedSoFar;

		public abstract int Count { get; }

		public abstract bool IsReadOnly { get; }

		public abstract void Add(T item);

		public abstract void Clear();

		public abstract bool Contains(T item);

		public abstract void CopyTo(T[] array, int arrayIndex);

		public abstract bool Remove(T item);

		public virtual IEnumerator<T> GetEnumerator()
		{
			IEnumerator<T> enumerated = _enumeratedSoFar.GetEnumerator();
			enumerated.Reset();
			while (enumerated.MoveNext()) {
				yield return enumerated.Current;
			}

			IEnumerator<T> newStuff = _source.GetEnumerator();
			while (newStuff.MoveNext())
			{
				_enumeratedSoFar.Add(newStuff.Current);
				yield return newStuff.Current;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}