using System;
using System.Collections.Generic;
using StandardDot.Dto.CoreServices;
using StandardDot.Enums;

namespace StandardDot.Abstract.CoreServices
{
    /// <summary>
    /// An interface for basic logging
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Logs a log object
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="log">The log to store</param>
        void Log<T>(Log<T> log)
            where T: new();

        /// <summary>
        /// Logs an exception with an optional message
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">A custom message in addtion to the exception, default none</param>
        /// <param name="logLevel">The log level, default Error</param>
        void LogException(Exception exception, string message = null, LogLevel logLevel = LogLevel.Error);

        /// <summary>
        /// Logs an exception with an optional message
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">A custom message in addtion to the exception, default none</param>
        /// <param name="logLevel">The log level, default Error</param>
        void LogExceptionWithObject<T>(Exception exception, T target, string message = null, LogLevel logLevel = LogLevel.Error)
            where T: new();

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="title">The title of the message</param>
        /// <param name="message">The message</param>
        /// <param name="logLevel">The log level</param>
        void LogMessage(string title, string message, LogLevel logLevel);

        /// <summary>
        /// Logs a message with an object
        /// </summary>
        /// <typeparam name="T">The target type (must be serializable)</typeparam>
        /// <param name="title">The title of the message</param>
        /// <param name="target">The object to serialize and include in the log</param>
        /// <param name="message">The message</param>
        /// <param name="logLevel">The log level</param>
        void LogMessage<T>(string title, T target, LogLevel logLevel, string message = null)
            where T: new();

        /// <summary>
        /// Get object to find all logs with exceptions
        /// </summary>
        /// <typeparam name="T">The target type for the logs (must be serializable), should typically be object</typeparam>
        /// <returns>IEnumerable that iterates through all logs with exceptions</returns>
        ILogEnumerable<T> GetLogs<T>()
            where T: new();
    }
}