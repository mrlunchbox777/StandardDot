using System.Runtime.Serialization;

namespace StandardDot.Dto.Exception
{
    /// <summary>
    /// A serializable version of <see cref="MethodImplAttributes" />.
    /// </summary>
    [DataContract]
    public enum SerializableMethodImplAttributes
    {
        [EnumMember]
        IL = 0,

        [EnumMember]
        Managed = 0,

        [EnumMember]
        Native = 1,

        [EnumMember]
        OPTIL = 2,

        [EnumMember]
        CodeTypeMask = 3,

        [EnumMember]
        Runtime = 3,

        [EnumMember]
        ManagedMask = 4,

        [EnumMember]
        Unmanaged = 4,

        [EnumMember]
        NoInlining = 8,

        [EnumMember]
        ForwardRef = 16,

        [EnumMember]
        Synchronized = 32,

        [EnumMember]
        NoOptimization = 64,

        [EnumMember]
        PreserveSig = 128,

        [EnumMember]
        AggressiveInlining = 256,

        [EnumMember]
        InternalCall = 4096,

        [EnumMember]
        MaxMethodImplVal = 65535
    }
}