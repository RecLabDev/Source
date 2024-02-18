using System;

namespace Theta.SDK.Utils.Logging
{
    /// <summary>
    /// Represents a logging utility.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log a debug message.
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        void Debug(string message, params object[] extra);

        /// <summary>
        /// Log a debug message.
        /// </summary>
        /// <param name="value"></param>
        void Debug(object value);

        /// <summary>
        /// Log an info message.
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        void Info(string message, params object[] extra);

        /// <summary>
        /// Log an info value.
        /// </summary>
        /// <param name="value">Value to be logged.</param>
        void Info(object value);

        /// <summary>
        /// Log a warning message.
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        void Warning(string message, params object[] extra);

        /// <summary>
        /// Log a warning value.
        /// </summary>
        /// <param name="value">Value to be logged.</param>
        void Warning(object value);

        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        void Error(string message, params object[] extra);

        /// <summary>
        /// Log an error value.
        /// </summary>
        /// <param name="value">Value to be logged.</param>
        void Error(object value);

        /// <summary>
        /// Log a fatality message.
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        void Fatal(string message, params object[] extra);

        /// <summary>
        /// Log a fatality value.
        /// </summary>
        /// <param name="value">Value to be logged.</param>
        void Fatal(object value);
    }

    /// <summary>
    /// Log to the Console.
    /// </summary>
    public static class TPLogger
    {
        /// <summary>
        /// Global logger instance. Set this during runtime startup and use
        /// by calling static methods. Debug, Info, etc methods.
        /// </summary>
        private static ILogger s_logger;

        /// <summary>
        /// Set the inner logger instance to an ILogger
        /// </summary>
        /// <param name="logger">The logger instance to use for global logging.</param>
        public static void Setup(ILogger logger)
        {
            s_logger = logger ?? new ConsoleLogger();
        }

        /// <summary>
        /// Log a debug message with the global logger instance..
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        public static void Debug(string message, params object[] extra)
        {
            s_logger.Debug(message, extra);
        }

        /// <summary>
        /// Log a debug value with the global logger instance..
        /// </summary>
        /// <param name="value">The value to log</param>
        public static void Debug(object value)
        {
            s_logger.Debug(value);
        }

        /// <summary>
        /// Log an info message with the global logger instance..
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        public static void Info(string message, params object[] extra)
        {
            s_logger.Info(message, extra);
        }

        /// <summary>
        /// Log an info message with the global logger instance..
        /// </summary>
        /// <param name="value">The value to log</param>
        public static void Info(object value)
        {
            s_logger.Info(value);
        }

        /// <summary>
        /// Log a warning message with the global logger instance..
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        public static void Warning(string message, params object[] extra)
        {
            s_logger.Warning(message, extra);
        }

        /// <summary>
        /// Log a warning message with the global logger instance..
        /// </summary>
        /// <param name="value">The value to log</param>
        public static void Warning(object value)
        {
            s_logger.Warning(value);
        }

        /// <summary>
        /// Log an error message with the global logger instance..
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        public static void Error(string message, params object[] extra)
        {
            s_logger.Error(message, extra);
        }

        /// <summary>
        /// Log an errorvaluet with the global logger instance..
        /// </summary>
        /// <param name="value">The value to log</param>
        public static void Error(object value)
        {
            s_logger.Error(value);
        }

        /// <summary>
        /// Log an error message with the global logger instance..
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        public static void Fatal(string message, params object[] extra)
        {
            s_logger.Fatal(message, extra);
        }

        /// <summary>
        /// Log an errorvaluet with the global logger instance..
        /// </summary>
        /// <param name="value">The value to log</param>
        public static void Fatal(object value)
        {
            s_logger.Fatal(value);
        }
    }

    //---
    /// <summary>
    /// Simple console-based logger.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Log a debug message to the console.
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        public void Debug(string message, params object[] extra)
        {
            System.Diagnostics.Debug.WriteLine(message, extra);
        }

        /// <summary>
        /// Log a debug object to the console.
        /// </summary>
        /// <param name="value">The value to log</param>
        public void Debug(object value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        /// <summary>
        /// Log an info message to the console.
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        public void Info(string message, params object[] extra)
        {
            Console.WriteLine(message, extra);
        }

        /// <summary>
        /// Log an info value to the console.
        /// </summary>
        /// <param name="value">The value to log</param>
        public void Info(object value)
        {
            Console.WriteLine(value);
        }

        /// <summary>
        /// Log a warning message to the console.
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        public void Warning(string message, params object[] extra)
        {
            Console.Error.WriteLine(message, extra);
        }

        /// <summary>
        /// Log a warning value to the console.
        /// </summary>
        /// <param name="value">The value to log</param>
        public void Warning(object value)
        {
            Console.Error.WriteLine(value);
        }

        /// <summary>
        /// Log an error message to the console.
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        public void Error(string message, params object[] extra)
        {
            Console.Error.WriteLine(message, extra);
        }

        /// <summary>
        /// Log an error object to the console.
        /// </summary>
        /// <param name="value">The value to log</param>
        public void Error(object value)
        {
            Console.Error.WriteLine(value);
        }

        /// <summary>
        /// Log an error message to the console.
        /// </summary>
        /// <param name="message">The formatting message to be logged</param>
        /// <param name="extra">Extra metadata</param>
        public void Fatal(string message, params object[] extra)
        {
            Console.Error.WriteLine(message, extra);
        }

        /// <summary>
        /// Log an error value to the console..
        /// </summary>
        /// <param name="value">The value to log</param>
        public void Fatal(object value)
        {
            Console.Error.WriteLine(value);
        }
    }
}
