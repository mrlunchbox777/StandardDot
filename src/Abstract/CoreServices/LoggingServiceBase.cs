using System;
using StandardDot.Dto.CoreServices;
using StandardDot.Dto.Exception;
using StandardDot.Enums;

namespace StandardDot.Abstract.CoreServices
{
    /// <summary>
    /// A Text File Logging Service
    /// </summary>
    public abstract class LoggingServiceBase : ILoggingService
    {
        /// <param name="serializationService">The serialization service to use</param>
        public LoggingServiceBase(ISerializationService serializationService)
        {
            SerializationService = serializationService;
        }

        protected ISerializationService SerializationService { get; }
        
        /// <summary>
        /// Logs a log object
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="log">The log to store</param>
        public abstract void Log<T>(Log<T> log)
            where T: new();

        /// <summary>
        /// Logs an exception with an optional message
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">A custom message in addtion to the exception, default none</param>
        /// <param name="logLevel">The log level, default Error</param>
        public virtual void LogException(Exception exception, string message = null, LogLevel logLevel = LogLevel.Error)
        {
            SerializableException serializableException = new SerializableException(exception);
            string description = "Exception Log - " + serializableException.Message;
            Log<object> log = new Log<object>
            {
                Target = null,
                TimeStamp = DateTime.UtcNow,
                Title = serializableException.Message,
                Message = message ?? description,
                LogLevel = logLevel,
                Exception = serializableException,
                Description = description
            };
            Log(log);
        }

        /// <summary>
        /// Logs an exception with an optional message
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">A custom message in addtion to the exception, default none</param>
        /// <param name="logLevel">The log level, default Error</param>
        public virtual void LogExceptionWithObject<T>(Exception exception, T target, string message = null, LogLevel logLevel = LogLevel.Error)
            where T: new()
        {
            SerializableException serializableException = new SerializableException(exception);
            string description = "Exception Log - " + serializableException.Message;
            Log<T> log = new Log<T>
            {
                Target = target,
                TimeStamp = DateTime.UtcNow,
                Title = serializableException.Message,
                Message = message ?? description,
                LogLevel = logLevel,
                Exception = serializableException,
                Description = description
            };
            Log<T>(log);
        }

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="title">The title of the message</param>
        /// <param name="message">The message</param>
        /// <param name="logLevel">The log level</param>
        public virtual void LogMessage(string title, string message, LogLevel logLevel)
        {
            Log<object> log = new Log<object>
            {
                Target = null,
                TimeStamp = DateTime.UtcNow,
                Title = title,
                Message = message,
                LogLevel = logLevel,
                Exception = null,
                Description = "Message Log"
            };
            Log(log);
        }

        /// <summary>
        /// Logs a message with an object
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="title">The title of the message</param>
        /// <param name="target">The object to serialize and include in the log</param>
        /// <param name="message">The message</param>
        /// <param name="logLevel">The log level</param>
        public virtual void LogMessage<T>(string title, T target, LogLevel logLevel, string message = null)
            where T: new()
        {
            string description = "Message Log with object - " + typeof(T).FullName;
            Log<T> log = new Log<T>
            {
                Target = target,
                TimeStamp = DateTime.UtcNow,
                Title = title,
                Message = message ?? description,
                LogLevel = logLevel,
                Exception = null,
                Description = description
            };
            Log(log);
        }

        /// <summary>
        /// Get object to find all logs (can be filtered)
        /// </summary>
        /// <typeparam name="T">The target type for the logs (must be serializable)</typeparam>
        /// <returns>IEnumerable that iterates through all logs</returns>
        public ILogEnumerable<T> GetLogs<T>()
            where T: new()
        {
            return BaseGetLogs<T>();
        }

        /// <summary>
        /// Get object to find all logs (can be filtered)
        /// </summary>
        /// <typeparam name="T">The target type for the logs (must be serializable), should typically be object</typeparam>
        /// <returns>IEnumerable that iterates through all logs</returns>
        protected abstract LogEnumerableBase<T> BaseGetLogs<T>()
            where T: new();
    }
}