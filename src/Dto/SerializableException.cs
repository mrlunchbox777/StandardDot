using System;
using System.Runtime.Serialization;

namespace StandardDot.Dto
{
    public class SerializableException
    {
        public SerializableException(Exception exception)
        {
            HelpLink = exception.HelpLink;
        }

        [DataMember(Name = "helpLink")]
        public string HelpLink { get; set; }
    }
}
