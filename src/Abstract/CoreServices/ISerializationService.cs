using System;
using System.IO;

namespace shoellibraries.Abstract.CoreServices
{
    ///
    public interface ISerializationService : IDisposable
    {
        string SerializeObject<T>(T Target);

        T DeserializeObject<T>(string Target);

        T DeserializeObject<T>(Stream Target);
    }
}