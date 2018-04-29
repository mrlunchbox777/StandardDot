using System;
using System.Runtime.Serialization;
using StandardDot.Dto.Exception;
using StandardDot.Enums;

namespace StandardDot.Dto.CoreServices
{
    /// <summary>
    /// A class that represents a generic log
    /// </summary>
    [DataContract]
    public class Log<T> : LogBase
        where T: new()
    {
        [DataMember(Name = "target")]
        public T Target
        {
            get => (T)TargetObject;
            set => TargetObject = value;
        }
    }
}