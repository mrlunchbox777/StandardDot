using System;
using System.IO;

namespace StandardDot.Abstract.CoreServices
{
    ///
    public interface ISerializationService : IDisposable
    {
        string SerializeObject<T>(T target);

        T DeserializeObject<T>(string target);

        T DeserializeObject<T>(Stream target);
    }
}