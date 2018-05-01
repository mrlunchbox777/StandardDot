
using System.Runtime.Serialization;

namespace StandardDot.Enums
{
    /// <summary>
    /// The log levels for logging services
    /// </summary>
    [DataContract]
    public enum HmacIsValidRequestResult
    {
        [EnumMember(Value = "General")]
        General = 0,

        [EnumMember(Value = "NoValidResouce")]
        NoValidResouce = 1,
        
        [EnumMember(Value = "UnableToFindAppId")]
        UnableToFindAppId = 2,
        
        [EnumMember(Value = "ReplayRequest")]
        ReplayRequest = 3,
        
        [EnumMember(Value = "SignaturesMismatch")]
        SignaturesMismatch = 4,

        [EnumMember(Value = "None")]
        NoError = 5,

        [EnumMember(Value = "NoHmacHeader")]
        NoHmacHeader = 6,

        [EnumMember(Value = "NotEnoughHeaderParts")]
        NotEnoughHeaderParts = 7,

        [EnumMember(Value = "BadNamespace")]
        BadNamespace = 8,

        [EnumMember(Value = "NotEnoughHeaderValueItems")]
        NotEnoughHeaderValueItems = 9
    }
}