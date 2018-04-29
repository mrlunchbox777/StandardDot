using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace StandardDot.Dto.Exception
{
    /// <summary>
    /// A class that can be used to serialize any exception. Validity of serialization is valued over complete data.
    /// </summary>
    [DataContract]
    public class SerializableException
    {
        public SerializableException() {}
        
        /// <param name="exception">The exception to convert to serializable form</param>
        /// <param name="includeTargetSite">If the target site should be included in the serialization, default true</param>
        /// <param name="includeData">If the data should be included in the serialization
        ///     <note>Even when included data is not guarunteed to be complete. It only includes primitives and <see cref="DataContractAttribute" />s</note>
        ///     , default false</param>
        public SerializableException(System.Exception exception, bool includeTargetSite = true, bool includeData = false)
        {
            if (exception == null)
            {
                Message = "No exception here. Serialized a null exception.";
                return;
            }

            Source = exception.Source;
            HelpLink = exception.HelpLink;
            StackTrace = exception.StackTrace;
            if (includeTargetSite)
            {
                TargetSite = exception.TargetSite == null
                    ? null
                    : new SerializableMethodBase(exception.TargetSite);
            }
            InnerException = exception.InnerException == null
                ? null
                : new SerializableException(exception.InnerException, includeTargetSite, includeData);
            Message = exception.Message;
            HResult = exception.HResult;
            if (includeData)
            {
                if (!(exception.Data?.Count > 0))
                {
                    return;
                }
                
                Data = new Dictionary<object, object>();
                foreach(DictionaryEntry item in exception.Data)
                {
                    if (item.Key == null)
                    {
                        continue;
                    }
                    Type keyType = item.Key.GetType();
                    Type valueType = item.Value?.GetType();
                    if (!(keyType.IsValueType || typeof(string).IsAssignableFrom(keyType) || typeof(decimal).IsAssignableFrom(keyType)
                        || keyType.CustomAttributes.FirstOrDefault(a => typeof(DataContractAttribute).IsAssignableFrom(a.AttributeType)) != null))
                    {
                        continue;
                    }
                    if (valueType == null || !(valueType.IsValueType || typeof(string).IsAssignableFrom(valueType) || typeof(decimal).IsAssignableFrom(valueType)
                        || valueType.CustomAttributes.FirstOrDefault(a => typeof(DataContractAttribute).IsAssignableFrom(a.AttributeType)) != null))
                    {
                        continue;
                    }
                    Data.Add(item.Key, item.Value);
                }
            }
        }

        [DataMember(Name = "source")]
        public virtual string Source { get; set; }

        [DataMember(Name = "helpLink")]
        public virtual string HelpLink { get; set; }
        
        [DataMember(Name = "stackTrace")]
        public virtual string StackTrace { get; set; }

        [DataMember(Name = "targetSite")]
        public virtual SerializableMethodBase TargetSite { get; set; }
        
        [DataMember(Name = "innerException")]
        public virtual SerializableException InnerException { get; set; }
        
        [DataMember(Name = "message")]
        public virtual string Message { get; set; }
        
        [DataMember(Name = "hResult")]
        public virtual int HResult { get; set; }
        
        [DataMember(Name = "data")]
        public virtual IDictionary Data { get; set; }
    }
}
