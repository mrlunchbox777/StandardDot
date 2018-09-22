
using System;
using System.Runtime.Serialization;

namespace StandardDot.Dto.Exception
{
	/// <summary>
	/// A serializable version of <see cref="RuntimeMethodHandle" />.
	/// </summary>
	[DataContract]
	public struct SerializableRuntimeMethodHandle
	{
		public SerializableRuntimeMethodHandle(RuntimeMethodHandle runtimeMethodHandle)
		{
			if (runtimeMethodHandle == null)
			{
				Value = IntPtr.Zero;
				return;
			}

			Value = runtimeMethodHandle.Value;
		}

		[DataMember(Name = "value")]
		public IntPtr Value { get; set; }
	}
}