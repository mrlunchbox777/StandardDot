using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;

namespace StandardDot.CoreServices.Logging
{
    /// <summary>
    /// An Enumerable to get text logs
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
        /// <param name="onlySerializeLogsOfTheCorrectType">Only serializes logs of the correct type, has a significant performance hit</param>
        public TextLogEnumerable(string path, ISerializationService serializationService, bool onlySerializeLogsOfTheCorrectType = false)
            : base(null)
        {
            AllLogPaths = Directory.EnumerateFiles(path).ToList();
            Visited = AllLogPaths.ToDictionary(p => p, p => (Log<T>)null);
            SerializationService = serializationService;
            OnlySerializeLogsOfTheCorrectType = onlySerializeLogsOfTheCorrectType;
        }

        protected virtual ISerializationService SerializationService { get; }

        protected bool OnlySerializeLogsOfTheCorrectType { get; }

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
                        try
                        {
                            log = GetLogFromFile(logPath);
                        }
                        catch (Exception)
                        {
                            if (OnlySerializeLogsOfTheCorrectType)
                            {
                                continue;
                            }
                            throw;
                        }
                    }
                    yield return log;
                }
            }
        }

        protected Log<T> GetLogFromFile(string logPath)
        {
            Log<T> value = SerializationService.DeserializeObject<Log<T>>(File.ReadAllText(logPath));
            Visited[logPath] = value;
            return value;
        }
    }
}