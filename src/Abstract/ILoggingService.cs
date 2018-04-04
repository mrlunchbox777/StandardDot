using System;
using StandardDot.Enums;

namespace StandardDot.Abstract
{
    public interface ILoggingService
    {
        string LogPath { get; }

        void LogException(Exception exception, string message = null, LogLevel logLevel = LogLevel.Error);

        void LogMessage(string title, string message, LogLevel logLevel);

        void LogMessage<T>(string title, T target, LogLevel logLevel, string message = null);
    }
}