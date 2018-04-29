using System;
using System.IO;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;

namespace StandardDot.CoreServices.Logging
{
    /// <summary>
    /// A Text File Logging Service
    /// </summary>
    public class TextLoggingService : LoggingServiceBase
    {
        /// <param name="logPath">The directory logs should be stored in (should end in /)</param>
        /// <param name="serializationService">The serialization service to use</param>
        /// <param name="logExtension">The extension to use for the logs</param>
        public TextLoggingService(string logPath, ISerializationService serializationService, string logExtension)
            : base(serializationService)
        {
            LogPath = logPath;
            LogExtension = logExtension;
        }

        public string LogPath { get; }

        public string LogExtension { get; set; }
        
        /// <summary>
        /// Logs a log object
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="log">The log to store</param>
        public override void Log<T>(Log<T> log)
        {
            string uniqueId = Guid.NewGuid().ToString("N");
            string logName = log.LogLevel + "_" + log.TimeStamp.ToFileTimeUtc() + "_" + uniqueId + LogExtension;
            string serializedLog = SerializationService.SerializeObject(log);
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            File.WriteAllText(LogPath + logName, serializedLog);
        }

        /// <summary>
        /// Get object to find all logs (can be filtered)
        /// </summary>
        /// <typeparam name="T">The target type for the logs (must be serializable)</typeparam>
        /// <returns>IEnumerable that iterates through all logs</returns>
        protected override LogEnumerableBase<T> BaseGetLogs<T>()
        {
            return new TextLogEnumerable<T>(LogPath, SerializationService);
        }

        /// <summary>
        /// Get object to find all logs
        /// </summary>
        /// <returns>IEnumerable that iterates through all logs</returns>
        protected override ILogBaseEnumerable BaseGetLogs()
        {
            return new TextLogBaseEnumerable(LogPath, SerializationService);
        }
    }
}