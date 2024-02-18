using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Theta.SDK.Utils
{
    /// <summary>
    /// Represents an executable command.
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Command display name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Command verb; Should be unique. Used for indexing and lookups.
        /// </summary>
        public abstract string Verb { get; }

        /// <summary>
        /// Summary for this command.
        /// </summary>
        public abstract string Summary { get; }

        /// <summary>
        /// Get usage info for this command.
        /// </summary>
        public abstract string Usage { get; }

        /// <summary>
        /// Collection of available sub-commands.
        /// </summary>
        public Commands SubCommands { get; } = new Commands();

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <returns>Task which resolves to a command Result code.</returns>
        public abstract Task<Result> Main(params string[] args);
    }

    //---
    /// <summary>
    /// Command factory utility.
    /// </summary>
    public class Commands : Dictionary<string, Command>
    {
        /// <summary>
        /// Register a new command with the collection.
        /// </summary>
        public void Register(Command command)
        {
            Add(command.Verb, command);
        }

        /// <summary>
        /// Get a command from the available commands.
        /// </summary>
        /// <param name="name">The name of the command</param>
        /// <returns>The command</returns>
        public Command Get(string name)
        {
            return this[name] ?? throw new CommandNotFoundException($"No command named {name}");
        }
    }

    //---
    /// <summary>
    /// Bad command input. Usually indicates a command is either missing
    /// or a sufficient command was not found.
    /// </summary>
    public class CommandFormatException : FormatException
    {
        /// <summary>
        /// Default CTOR
        /// </summary>
        /// <param name="message">Exception message</param>
        public CommandFormatException(string message) : base(message)
        { }

        /// <summary>
        /// Default CTOR
        /// </summary>
        /// <param name="message">Exception message</param>
        public CommandFormatException(string message, string usage) : base($"{message};\nUsage: {usage}")
        { }
    }

    //---
    /// <summary>
    /// Bad command input. Usually indicates a command is either missing
    /// or a sufficient command was not found.
    /// </summary>
    public class CommandNotFoundException : KeyNotFoundException
    {
        /// <summary>
        /// Default CTOR
        /// </summary>
        /// <param name="message">Exception message</param>
        public CommandNotFoundException(string message) : base(message)
        { }
    }
}
