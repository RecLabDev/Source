using System;
using System.Threading.Tasks;

// using Aby.SDK.Utils;

namespace Aby.SDK
{
    /// <summary>
    /// Aby SDK environment variables.
    /// </summary>
    public static class Env
    {
        //..
    }

    /// <summary>
    /// Configuration for the Aby SDK.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// The maximum amount of time we should wait for asyncronous tasks to complete.
        /// </summary>
        public static readonly TimeSpan MaxTimeout = new TimeSpan(0, 0, 30);

        /// <summary>
        /// The name of the AbySDK assembly.
        /// TODO: Get this from the project reference (if possible)
        /// </summary>
        public static readonly string AssemblyName = "AbySDK";
    }

    /// <summary>
    /// Aby SDK errors.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        /// <summary>
        /// Generic error.
        /// </summary>
        Generic = 1,
    }

    /// <summary>
    /// Extension for `ErrorCode`.
    /// </summary>
    //public class ErrorCodeExtension
    //{
    //    /// <summary>
    //    /// Cast to int (e.g., for cleaner main returns, etc.).
    //    /// </summary>
    //    public static implicit operator int(ErrorCode code)
    //    {
    //        return (int)code;
    //    }
    //}
}
