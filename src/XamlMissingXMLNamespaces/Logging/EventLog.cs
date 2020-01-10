namespace XamlMissingXMLNamespaces.Logging
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;
    using Storage;

    /// <summary>
    /// Defines a helper for console event logging to a file.
    /// </summary>
    public static class EventLog
    {
        private const string LogFormat = "{0:dd-MM-yyyy HH\\:mm\\:ss\\:ffff}\tMessage: '{1}'";

        private static StreamWriter logWriter;

        private static ConsoleColor DefaultColor => Console.ForegroundColor;

        private static ConsoleColor ErrorColor => ConsoleColor.Red;

        /// <summary>
        /// Starts the event logging to a file.
        /// </summary>
        public static void StartFileLogging()
        {
            string filePath = GetLogFile();
            logWriter = new StreamWriter(filePath, true) {AutoFlush = true};
        }

        /// <summary>
        /// Stops the event logging to a file.
        /// </summary>
        public static void StopFileLogging()
        {
            try
            {
                logWriter.Close();
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        /// <summary>
        /// Writes a debug message to the event log.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        public static void Debug(string message)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Log(LogType.Debug, message);
            }
        }

        /// <summary>
        /// Writes an exception to the event log as a debug message.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public static void Debug(string message, Exception ex)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Log(LogType.Debug, $"{message} - Error: '{ex}'");
            }
        }

        /// <summary>
        /// Writes an exception to the event log as a debug message.
        /// </summary>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public static void Debug(Exception ex)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Log(LogType.Debug, $"Error: '{ex}'");
            }
        }

        /// <summary>
        /// Writes a generic information message to the event log.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        public static void Info(string message)
        {
            Log(LogType.Info, message);
        }

        /// <summary>
        /// Writes an exception to the event log as a generic information message.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public static void Info(string message, Exception ex)
        {
            Log(LogType.Info, $"{message} - Error: '{ex}'");
        }

        /// <summary>
        /// Writes an exception to the event log as a generic information message.
        /// </summary>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public static void Info(Exception ex)
        {
            Log(LogType.Info, $"Error: '{ex}'");
        }

        /// <summary>
        /// Writes a warning message to the event log.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        public static void Warning(string message)
        {
            Log(LogType.Warning, message);
        }

        /// <summary>
        /// Writes an exception to the event log as a warning message.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public static void Warning(string message, Exception ex)
        {
            Log(LogType.Warning, $"{message} - Error: '{ex}'");
        }

        /// <summary>
        /// Writes an exception to the event log as a warning message.
        /// </summary>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public static void Warning(Exception ex)
        {
            Log(LogType.Warning, $"Error: '{ex}'");
        }

        /// <summary>
        /// Writes an error message to the event log.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        public static void Error(string message)
        {
            Log(LogType.Error, message);
        }

        /// <summary>
        /// Writes an exception to the event log as an error message.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public static void Error(string message, Exception ex)
        {
            Log(LogType.Error, $"{message} - Error: '{ex}'");
        }

        /// <summary>
        /// Writes an exception to the event log as an error message.
        /// </summary>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public static void Error(Exception ex)
        {
            Log(LogType.Error, $"Error: '{ex}'");
        }

        /// <summary>
        /// Writes a critical error message to the event log.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        public static void Critical(string message)
        {
            Log(LogType.Critical, message);
        }

        /// <summary>
        /// Writes an exception to the event log as a critical message.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public static void Critical(string message, Exception ex)
        {
            Log(LogType.Critical, $"{message} - Error: '{ex}'");
        }

        /// <summary>
        /// Writes an exception to the event log as a critical message.
        /// </summary>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public static void Critical(Exception ex)
        {
            Log(LogType.Error, $"Error: '{ex}'");
        }

        private static void Log(LogType type, string message)
        {
            string logMessage = string.Format(LogFormat, DateTime.Now, message);

            logWriter.WriteLine(logMessage);

            Console.ForegroundColor = type == LogType.Error || type == LogType.Critical ? ErrorColor : DefaultColor;
            Console.WriteLine(logMessage);
            Console.ForegroundColor = DefaultColor;
        }

        private static string GetLogFile()
        {
            string logFileName = $"Log-{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            return FileStorageHelper.CreatePathForApplicationFile(logFileName, "Logs");
        }
    }
}