using System;

namespace Theta.SDK.Utils
{
    /// <summary>
    /// Working with System.Environment can be both obtuse and verbose.
    /// These utils should (hopefully) make that less true.
    /// </summary>
    public static class Env
    {
        /// <summary>
        /// The target for getting and setting environment variables.
        /// TODO: Unit test(s) to ensure this stays in-sync with System.EnvironmentVariableTarget.
        /// </summary>
        public enum Target
        {
            Process = EnvironmentVariableTarget.Process,
            User = EnvironmentVariableTarget.User,
            Machine = EnvironmentVariableTarget.Machine,
        }

        /// <summary>
        /// Get an environment variable from the `process` target.
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <returns>The variable value</returns>
        public static string GetVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, (EnvironmentVariableTarget)Target.Process);
        }

        /// <summary>
        /// Get an environment variable from a specified target.
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="target">The target environment</param>
        /// <returns>The variable value</returns>
        public static string GetVariable(string name, Target target)
        {
            return System.Environment.GetEnvironmentVariable(name, (EnvironmentVariableTarget)target);
        }

        /// <summary>
        /// Set an environment variable for the `process` target.
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="value">The new variable value</param>
        public static void SetVariable(string name, string value)
        {
            System.Environment.SetEnvironmentVariable(name, value, (EnvironmentVariableTarget)Target.Process);
        }

        /// <summary>
        /// Get an environment variable from a specified target.
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="target">The target environment</param>
        /// <returns>The variable value</returns>
        public static void SetVariable(string name, string value, Target target)
        {
            System.Environment.SetEnvironmentVariable(name, value, (EnvironmentVariableTarget)target);
        }
    }
}
