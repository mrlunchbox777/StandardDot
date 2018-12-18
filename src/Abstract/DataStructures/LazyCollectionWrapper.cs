using System.Collections.Generic;

namespace StandardDot.Abstract.DataStructures
{
	public class LazyCollectionWrapper<T> : BaseLazyCollection<T>
	{
		public LazyCollectionWrapper(ICollection<T> source) : base(source)
		{
		}

		ICollection<T> SourceCollection
		{
			get
			{
				if (_source == null)
				{
					_source = new List<T>();
				}
				return (ICollection<T>)_source;
			}
		}

		public override int Count => SourceCollection.Count;

		public override bool IsReadOnly => SourceCollection.IsReadOnly;

		public override void Add(T item)
		{
			SourceCollection.Add(item);
		}

		public override void Clear()
		{
			SourceCollection.Clear();
		}

		public override bool Contains(T item)
		{
			return SourceCollection.Contains(item);
		}

		public override void CopyTo(T[] array, int arrayIndex)
		{
			SourceCollection.CopyTo(array, arrayIndex);
		}

		public override bool Remove(T item)
		{
			return SourceCollection.Remove(item);
		}

		public override IEnumerator<T> GetEnumerator()
		{
			return SourceCollection.GetEnumerator();
		}
	}
}
