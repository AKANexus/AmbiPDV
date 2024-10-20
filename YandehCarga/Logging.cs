﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YandehCarga
{
    public sealed class Logger
    {
        #region Log File Writing
        public static bool Listening { get; private set; }
        public static FileInfo TargetLogFile { get; private set; }
        public static DirectoryInfo TargetDirectory => TargetLogFile?.Directory;

        public static bool LogToConsole = false;
        public static int BatchInterval = 1000;
        public static bool IgnoreDebug = false;

        private static readonly Timer Timer = new Timer(Tick);
        private static readonly StringBuilder LogQueue = new StringBuilder();

        public static void Start(FileInfo targetLogFile)
        {
            if (Listening)
                return;

            Listening = true;
            TargetLogFile = targetLogFile;
            VerifyTargetDirectory();

            Timer.Change(BatchInterval, Timeout.Infinite); // A one-off tick event that is reset every time.
        }

        private static void VerifyTargetDirectory()
        {
            if (TargetDirectory == null)
                throw new DirectoryNotFoundException("Target logging directory not found.");

            TargetDirectory.Refresh();
            if (!TargetDirectory.Exists)
                TargetDirectory.Create();
        }

        private static void Tick(object state)
        {
            try
            {
                var logMessage = "";
                lock (LogQueue)
                {
                    logMessage = LogQueue.ToString();
                    LogQueue.Length = 0;
                }

                if (string.IsNullOrEmpty(logMessage))
                    return;

                if (LogToConsole)
                    Console.Write(logMessage);

                VerifyTargetDirectory(); // File may be deleted after initialization.
                File.AppendAllText(TargetLogFile.FullName, logMessage);
            }
            finally
            {
                if (Listening)
                    Timer.Change(BatchInterval, Timeout.Infinite); // Reset timer for next tick.
            }
        }

        public static void ShutDown()
        {
            if (!Listening)
                return;

            Listening = false;
            Timer.Dispose();
            Tick(null); // Flush.
        }
        #endregion

        public readonly string Name;
        public EventHandler<LogMessageInfo> LogMessageAdded;
        private bool _startedErrorShown = false;

        public const string DEBUG = "DEBUG";
        public const string INFO = "INFO";
        public const string WARN = "WARN";
        public const string ERROR = "ERROR";

        public Logger(Type t) : this(t.Name)
        {
        }

        public Logger(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Logs a debug message if "IgnoreDebug" is set as false
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
            if (IgnoreDebug)
                return;

            Log(DEBUG, message);
            System.Diagnostics.Debug.WriteLine($"DEBUG>> {message}");
        }

        /// <summary>
        /// Logs an information message
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            Log(INFO, message);
            System.Diagnostics.Debug.WriteLine($"INFO>> {message}");
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Warn(string message, Exception ex = null)
        {
            Log(WARN, message, ex);
            System.Diagnostics.Debug.WriteLine($"WARN>> {message}");
            if (ex is not null)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message, Exception ex = null)
        {
            Log(ERROR, message, ex);
            System.Diagnostics.Debug.WriteLine($"ERROR>> {message}");
            if (ex is not null)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }

        public void Log(string level, string message, Exception ex = null)
        {
            if (!CheckListening())
                return;

            if (ex != null)
                message += string.Format("\r\n{0}({1})\r\n{2}", ex, ex.Message, ex.StackTrace);

            var info = new LogMessageInfo(level, Name, message);
            var msg = info.ToString();

            lock (LogQueue)
            {
                LogQueue.AppendLine(msg);
            }

            var evnt = LogMessageAdded;
            if (evnt != null)
                evnt.Invoke(this, info); // Block caller.
        }

        private bool CheckListening()
        {
            if (Listening)
                return true;

            if (!_startedErrorShown)
            {
                Console.WriteLine("Logging has not been started.");
                _startedErrorShown = true; // No need to excessively repeat this message.
            }

            return false;
        }
    }

    public sealed class LogMessageInfo : EventArgs
    {
        public readonly DateTime Timestamp;
        public readonly string ThreadId;
        public readonly string Level;
        public readonly string Logger;
        public readonly string Message;

        public bool IsError => YandehCarga.Logger.ERROR.Equals(Level, StringComparison.Ordinal);
        public bool IsWarning => YandehCarga.Logger.WARN.Equals(Level, StringComparison.Ordinal);
        public bool IsInformation => YandehCarga.Logger.INFO.Equals(Level, StringComparison.Ordinal);
        public bool IsDebug => YandehCarga.Logger.DEBUG.Equals(Level, StringComparison.Ordinal);

        public LogMessageInfo(string level, string logger, string message)
        {
            Timestamp = DateTime.Now;
            var thread = Thread.CurrentThread;
            ThreadId = string.IsNullOrEmpty(thread.Name) ? thread.ManagedThreadId.ToString() : thread.Name;
            Level = level;
            Logger = logger;
            Message = message;
        }

        public override string ToString()
        {
            return string.Format("{0:yyyy/MM/dd HH:mm:ss.fff} {1} {2} {3} {4}",
                Timestamp, ThreadId, Logger, Level, Message);
        }
    }

}
