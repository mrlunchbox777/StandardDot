using System.Runtime.Serialization;

namespace StandardDot.Enums
{
    /// <summary>
    /// The log levels for logging services
    /// </summary>
    [DataContract]
    public enum LogLevel
    {
        [EnumMember(Value = "Debug")]
        Debug = 0,
        
        [EnumMember(Value = "Info")]
        Info = 1,
        
        [EnumMember(Value = "Warning")]
        Warning = 2,
        
        [EnumMember(Value = "Error")]
        Error = 3
    }
}