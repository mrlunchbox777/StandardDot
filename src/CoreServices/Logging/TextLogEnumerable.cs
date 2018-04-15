using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;

namespace StandardDot.CoreServices.Logging
{
    /// <summary>
    /// A Base Enumerator to get logs
    /// </summary>
    /// <typeparam name="T">The target type for the logs (must be serializable), should typically be object</typeparam>
    public class TextLogEnumerable<T> : LogEnumerableBase<T>
        where T : new()
    {
        /// <param name="source">The source that the enumerable should represent</param>
        public TextLogEnumerable(IEnumerable<Log<T>> source)
            : base(source)
        { }

        /// <param name="source">The source that the enumerable should represent</param>
        public TextLogEnumerable(ILogEnumerable<T> source)
            : base(source)
        { }

        /// <param name="path">The directory logs should be stored in (should end in /)</param>
        /// <param name="serializationService">The serialization service to use</param>
        public TextLogEnumerable(string path, ISerializationService serializationService)
            : base(null)
        {
            AllLogPaths = Directory.EnumerateFiles(path).ToList();
            Visited = AllLogPaths.ToDictionary(p => p, p => (Log<T>)null);
            SerializationService = serializationService;
        }

        protected virtual ISerializationService SerializationService { get; }

        protected List<string> AllLogPaths { get; }
        
        protected Dictionary<string, Log<T>> Visited { get; }

        public override IEnumerator<Log<T>> GetEnumerator()
        {
            if (Visited == null)
            {
                Source.GetEnumerator();
            }
            else
            {
                foreach(string logPath in AllLogPaths)
                {
                    if (!Visited.ContainsKey(logPath))
                    {
                        continue;
                    }
                    Log<T> log = Visited[logPath];
                    if (log == null)
                    {
                        Log<T> value = SerializationService.DeserializeObject<Log<T>>(File.ReadAllText(logPath));
                        Visited[logPath] = value;
                        yield return value;
                    }
                    yield return log;
                }
            }
        }
    }
}