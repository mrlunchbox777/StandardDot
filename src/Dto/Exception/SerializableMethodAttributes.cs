using System;
using System.Runtime.Serialization;

namespace StandardDot.Dto.Exception
{
	/// <summary>
	/// A serializable version of <see cref="MethodAttributes" />.
	/// </summary>
	[DataContract]
	[Flags]
	public enum SerializableMethodAttributes
	{
		[EnumMember]
		PrivateScope = 0,

		[EnumMember]
		ReuseSlot = 0,

		[EnumMember]
		Private = 1,

		[EnumMember]
		FamANDAssem = 2,

		[EnumMember]
		Assembly = 3,

		[EnumMember]
		Family = 4,

		[EnumMember]
		FamORAssem = 5,

		[EnumMember]
		Public = 6,

		[EnumMember]
		MemberAccessMask = 7,

		[EnumMember]
		UnmanagedExport = 8,

		[EnumMember]
		Static = 16,

		[EnumMember]
		Final = 32,

		[EnumMember]
		Virtual = 64,

		[EnumMember]
		HideBySig = 128,

		[EnumMember]
		NewSlot = 256,

		[EnumMember]
		VtableLayoutMask = 256,

		[EnumMember]
		CheckAccessOnOverride = 512,

		[EnumMember]
		Abstract = 1024,

		[EnumMember]
		SpecialName = 2048,

		[EnumMember]
		RTSpecialName = 4096,

		[EnumMember]
		PinvokeImpl = 8192,

		[EnumMember]
		HasSecurity = 16384,

		[EnumMember]
		RequireSecObject = 32768,

		[EnumMember]
		ReservedMask = 53248
	}
}