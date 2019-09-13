
using System;
using System.Runtime.Serialization;

namespace StandardDot.Dto.Exception
{
	/// <summary>
	/// A serializable version of <see cref="System.RuntimeMethodHandle" />.
	/// </summary>
	[DataContract]
	public struct SerializableRuntimeMethodHandle
	{
		/// <param name="runtimeMethodHandle">The runtimeMethodHandle info to serialize</param>
		public SerializableRuntimeMethodHandle(RuntimeMethodHandle runtimeMethodHandle)
		{
			// runtimeMethodHandle can't be null
			Value = runtimeMethodHandle.Value;
		}

		/// <summary>
		/// The <see cref="System.IntPtr" /> that represents the method handle.
		/// </summary>
		[DataMember(Name = "value")]
		public IntPtr Value { get; set; }
	}
}