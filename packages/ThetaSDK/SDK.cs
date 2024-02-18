using System;
using System.Threading.Tasks;

// using Theta.SDK.Utils;

namespace Theta.SDK
{
    /// <summary>
    /// Theta SDK environment variables.
    /// </summary>
    public static class Env
    {
        //..
    }

    /// <summary>
    /// Configuration for the Theta SDK.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// The maximum amount of time we should wait for asyncronous tasks to complete.
        /// </summary>
        public static readonly TimeSpan MaxTimeout = new TimeSpan(0, 0, 30);

        /// <summary>
        /// The name of the ThetaSDK assembly.
        /// TODO: Get this from the project reference (if possible)
        /// </summary>
        public static readonly string AssemblyName = "ThetaSDK";
    }

    /// <summary>
    /// Theta SDK errors.
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
