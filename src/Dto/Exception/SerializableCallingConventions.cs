using System;
using System.Runtime.Serialization;

namespace StandardDot.Dto.Exception
{
	/// <summary>
	/// A serializable version of <see cref="CallingConventions" />.
	/// </summary>
	[DataContract]
	[Flags]
	public enum SerialiableCallingConventions
	{
		[EnumMember]
		Standard = 1,

		[EnumMember]
		VarArgs = 2,

		[EnumMember]
		Any = 3,

		[EnumMember]
		HasThis = 32,

		[EnumMember]
		ExplicitThis = 64
	}
}