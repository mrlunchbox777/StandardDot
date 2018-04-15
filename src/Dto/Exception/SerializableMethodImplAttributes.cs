using System.Runtime.Serialization;

namespace StandardDot.Dto.Exception
{
    /// <summary>
    /// A serializable version of <see cref="MethodImplAttributes" />.
    /// </summary>
    [DataContract]
    public enum SerializableMethodImplAttributes
    {
        [EnumMember(Value = "IL")]
        IL = 0,

        [EnumMember(Value = "Managed")]
        Managed = 0,

        [EnumMember(Value = "Native")]
        Native = 1,

        [EnumMember(Value = "OPTIL")]
        OPTIL = 2,

        [EnumMember(Value = "CodeTypeMask")]
        CodeTypeMask = 3,

        [EnumMember(Value = "Runtime")]
        Runtime = 3,

        [EnumMember(Value = "ManagedMask")]
        ManagedMask = 4,

        [EnumMember(Value = "Unmanaged")]
        Unmanaged = 4,

        [EnumMember(Value = "NoInlining")]
        NoInlining = 8,

        [EnumMember(Value = "ForwardRef")]
        ForwardRef = 16,

        [EnumMember(Value = "Synchronized")]
        Synchronized = 32,

        [EnumMember(Value = "NoOptimization")]
        NoOptimization = 64,

        [EnumMember(Value = "PreserveSig")]
        PreserveSig = 128,

        [EnumMember(Value = "AggressiveInlining")]
        AggressiveInlining = 256,

        [EnumMember(Value = "InternalCall")]
        InternalCall = 4096,

        [EnumMember(Value = "MaxMethodImplVal")]
        MaxMethodImplVal = 65535
    }
}