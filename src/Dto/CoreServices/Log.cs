using System;
using System.Runtime.Serialization;
using StandardDot.Dto.Exception;
using StandardDot.Enums;

namespace StandardDot.Dto.CoreServices
{
    /// <summary>
    /// A class that represents a basic log
    /// </summary>
    [DataContract]
    public class Log<T>
        where T: new()
    {
        [DataMember(Name = "exception")]
        public SerializableException Exception { get; set; }

        [DataMember(Name = "target")]
        public T Target { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "timeStamp")]
        public DateTime TimeStamp { get; set; }

        [DataMember(Name = "logLevel")]
        public LogLevel LogLevel { get; set; }
    }
}