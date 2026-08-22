//-----------------------------------------------------------------------
// <copyright file="FileLoggerProvider.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Logging
{
    using System;
    using System.IO;
    using System.Text;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A lightweight file logger provider that writes log entries to a daily-rotating
    /// log file in the application's data directory
    /// (<c>%APPDATA%\AIFileProcessor\logs\</c>). Each log line is flushed immediately
    /// so logs are preserved even on crash. Thread-safe via a lock on the file stream.
    /// </summary>
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logDirectory;
        private readonly object _lock = new();
        private StreamWriter? _currentWriter;
        private string? _currentLogDate;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLoggerProvider"/> class.
        /// </summary>
        /// <param name="baseDirectory">The base directory for log files. Defaults to
        /// <c>%APPDATA%\AIFileProcessor\logs</c> when null or empty.</param>
        public FileLoggerProvider(string? baseDirectory = null)
        {
            _logDirectory = string.IsNullOrWhiteSpace(baseDirectory)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIFileProcessor", "logs")
                : baseDirectory;

            Directory.CreateDirectory(_logDirectory);
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance that writes to the log file.
        /// </summary>
        /// <param name="categoryName">The category name (typically the fully-qualified type name).</param>
        /// <returns>A file-backed logger instance.</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, this);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            lock (_lock)
            {
                _currentWriter?.Flush();
                _currentWriter?.Dispose();
                _currentWriter = null;
            }

            _disposed = true;
        }

        internal void WriteLog(string categoryName, LogLevel logLevel, EventId eventId, string message, Exception? exception)
        {
            var timestamp = DateTime.Now;
            string dateKey = timestamp.ToString("yyyy-MM-dd");

            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                // Rotate to a new file each day.
                if (_currentLogDate != dateKey)
                {
                    _currentWriter?.Flush();
                    _currentWriter?.Dispose();

                    string logFile = Path.Combine(_logDirectory, $"AIFileProcessor_{dateKey}.log");
                    _currentWriter = new StreamWriter(logFile, append: true, Encoding.UTF8) { AutoFlush = true };
                    _currentLogDate = dateKey;
                }

                string levelText = logLevel switch
                {
                    LogLevel.Trace => "TRC",
                    LogLevel.Debug => "DBG",
                    LogLevel.Information => "INF",
                    LogLevel.Warning => "WRN",
                    LogLevel.Error => "ERR",
                    LogLevel.Critical => "CRT",
                    LogLevel.None => "   ",
                    _ => "???",
                };

                var sb = new StringBuilder();
                sb.Append(timestamp.ToString("HH:mm:ss.fff"));
                sb.Append(" [");
                sb.Append(levelText);
                sb.Append("] ");
                sb.Append(categoryName);
                sb.Append(": ");
                sb.Append(message);

                if (exception != null)
                {
                    sb.AppendLine();
                    sb.Append("  -> ");
                    sb.Append(exception.ToString());
                }

                _currentWriter?.WriteLine(sb.ToString());
            }
        }

        private sealed class FileLogger(string categoryName, FileLoggerProvider provider) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
                => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                provider.WriteLog(categoryName, logLevel, eventId, formatter(state, exception), exception);
            }
        }
    }
}