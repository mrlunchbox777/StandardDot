using System;
using System.IO;
using shoellibraries.CoreExtensions;
using shoellibraries.CoreExtensions.Object;

namespace shoellibraries.Abstract.CoreServices
{
    ///
    public class Json : ISerializationService
    {
        public T DeserializeObject<T>(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return default(T);
            }
            return target.DeserializeJson<T>();
        }

        public T DeserializeObject<T>(Stream target)
        {
            if (target == null)
            {
                return default(T);
            }
            return target.GetStringFromStream().DeserializeJson<T>();
        }

        public void Dispose()
        {
            // nothing to dispose of
        }

        public string SerializeObject<T>(T target)
        {
            if (target == null)
            {
                return "";
            }
            return target.SerializeJson();
        }
    }
}