﻿using Nuke.Common.OutputSinks;
using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nuke.Common.Logging
{
    internal class InMemoryLogger : ILogger
    {
        private static object _flushLock = new object();

        public bool AutoFlush { get; } = true;

        public LogLevel LogLevel { get; }

        public OutputSink Sink { get; }

        private List<LogEntry> Entries { get; } = new List<LogEntry>();

        public InMemoryLogger(LogLevel logLevel, OutputSink sink, bool autoFlush)
        {
            AutoFlush = autoFlush;
            Sink = sink;
            LogLevel = logLevel;
        }

        /// <summary>
        /// Logs a message with <paramref name="level"/> severity if it is lower or equal to <see cref="NukeBuild.LogLevel"/>.
        /// </summary>
        public void Log(LogLevel level, string text = null, Exception exception = null)
        {
            lock (Entries)
            {
                if (AutoFlush)
                    Flush(level, text, exception);
                else
                    Entries.Add(new LogEntry(level, text, exception));
            }
        }

        public void Flush()
        {
            lock (_flushLock)
            {
                lock (Entries)
                {
                    Entries.ForEach(entry => Flush(entry.Level, entry.Message, entry.Exception));
                    Entries.Clear();
                }
            }
        }

        private void Flush(LogLevel level, string text, Exception exception = null)
        {
            if (LogLevel > level)
                return;

            switch (level)
            {
                case LogLevel.Trace:
                    Sink.WriteTrace(text);
                    break;

                case LogLevel.Normal:
                    Sink.WriteNormal(text);
                    break;

                case LogLevel.Information:
                    Sink.WriteInformation(text);
                    break;

                case LogLevel.Warning:
                    if (exception != null)
                        HandleException(exception, Sink.WriteAndReportWarning);
                    else
                        Sink.WriteAndReportWarning(text);
                    break;

                case LogLevel.Error:
                    if (exception != null)
                        HandleException(exception, Sink.WriteAndReportError);
                    else
                        Sink.WriteAndReportError(text);
                    break;
            }
        }

        /// <summary>
        /// Logs a message as information if <see cref="LogLevel"/> is lower or equal to <see cref="Common.LogLevel.Normal"/>.
        /// </summary>
        public void Normal(string text = null)
        {
            Log(LogLevel.Normal, text);
        }

        /// <summary>
        /// Logs a message as trace if <see cref="LogLevel"/> is equal to <see cref="Nuke.Common.LogLevel.Trace"/>.
        /// </summary>
        public void Trace(string text = null)
        {
            Log(LogLevel.Trace, text);
        }

        /// <summary>
        /// Logs a message as information if <see cref="LogLevel"/> is lower or equal to <see cref="Common.LogLevel.Normal"/>.
        /// </summary>
        public void Info(string text = null)
        {
            Log(LogLevel.Information, text);
        }

        /// <summary>
        /// Logs a message as warning if <see cref="LogLevel"/> is lower or equal to <see cref="Nuke.Common.LogLevel.Warning"/>.
        /// </summary>
        public void Warn(string text = null)
        {
            Log(LogLevel.Warning, text);
        }

        /// <summary>
        /// Logs an exception as warning if <see cref="LogLevel"/> is lower or equal to <see cref="Nuke.Common.LogLevel.Warning"/>.
        /// </summary>
        public void Warn(Exception exception)
        {
            Log(LogLevel.Warning, exception: exception);
        }

        /// <summary>
        /// Logs a message as error.
        /// </summary>
        public void Error(string text = null)
        {
            Log(LogLevel.Error, text);
        }

        /// <summary>
        /// Logs an exception as error.
        /// </summary>
        public void Error(Exception exception)
        {
            Log(LogLevel.Error, exception: exception);
        }

        private static void HandleException(Exception exception, Action<string, string> exceptionOutput, string prefix = null)
        {
            switch (exception)
            {
                case AggregateException ex:
                    var exceptions = ex.Flatten().InnerExceptions;
                    exceptions.ForEach((x, i) => HandleException(x, exceptionOutput, $"#{i + 1}/{exceptions.Count}: "));
                    break;

                case TargetInvocationException ex:
                    HandleException(ex.InnerException, exceptionOutput);
                    break;

                case TypeInitializationException ex:
                    HandleException(ex.InnerException, exceptionOutput);
                    break;

                default:
                    exceptionOutput(prefix + exception.Message, exception.StackTrace + EnvironmentInfo.NewLine);
                    break;
            }
        }

        public string PeekLastMessage()
        {
            if (Entries.Count == 0)
                return null;

            try
            {
                string current = null;
                var index = Entries.Count - 1;

                while (index > 0 && string.IsNullOrWhiteSpace(current))
                {
                    current = Entries[index--].Message;
                }

                return current;
            }
            catch
            {
                return null;
            }
        }
    }
}
