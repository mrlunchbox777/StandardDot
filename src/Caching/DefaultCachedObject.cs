using System;
using System.Runtime.Serialization;
using StandardDot.Abstract.Caching;

namespace StandardDot.Caching
{
	/// <summary>
	/// A Basic Caching Object Wrapper
	/// </summary>
	/// <typeparam name="T">The wrapped object Type</typeparam>
	[DataContract]
	public class DefaultCachedObject<T> : ICachedObject<T>
	{
		[DataMember(Name = "cachedTime")]
		public DateTime CachedTime { get; set; }

		[DataMember(Name = "expireTime")]
		public DateTime ExpireTime { get; set; }

		[DataMember(Name = "value")]
		public T Value { get; set; }

		[DataMember(Name = "retrievedSuccesfully")]
		public bool RetrievedSuccesfully { get; set; }

		[IgnoreDataMember]
		public object UntypedValue { get => Value; set => Value = (value is T ? (T)value : default(T)); }
	}
}