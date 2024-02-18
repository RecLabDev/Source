using System;

namespace Theta.SDK.Utils
{
    /// <summary>
    /// A parsed command-line flag.
    /// </summary>
    /// <typeparam name="T">The flag value type</typeparam>
    public struct Flag<T>
    {
        /// <summary>
        /// Inner value.
        /// </summary>
        public T Value { get; set; }
    }

    /// <summary>
    /// A set of flags of a particular value.
    /// </summary>
    public class Flags
    {
        /// <summary>
        /// Get a flag value of a particular type.
        /// </summary>
        /// <typeparam name="T">The flag value type</typeparam>
        /// <param name="name">The flag name</param>
        /// <returns>A flag value</returns>
        public Flag<T> Get<T>(string name)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
