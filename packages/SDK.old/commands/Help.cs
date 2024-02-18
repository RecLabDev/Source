using System;
using System.Threading.Tasks;

using Theta.SDK.Utils;

namespace Theta.SDK.Commands
{
    /// <summary>
    /// Help command definition.
    /// </summary>
    public sealed class Help : Command
    {
        public override string Name => "Explore";
        public override string Verb => "explore";
        public override string Summary => "Explore Theta SDK resources.";
        public override string Usage => $"TODO";

        /// <summary>
        /// Collection of commands registered to the current Theta context.
        /// </summary>
        public CommandCollection Commands { get; }

        /// <summary>
        /// Default CTOR.
        /// </summary>
        public Help(CommandCollection commands)
        {
            Commands = commands;
        }

        /// <summary>
        /// Mount resources required to execute the command.
        /// Runs immediately before 
        /// </summary>
        public Task<Result> Mount()
        {
            return Task.FromResult(Result.NoOp);
        }

        /// <summary>
        /// Cleanup the command.
        /// </summary>
        public void Dispose()
        { }

        /// <summary>
        /// Print inspection output to the console.
        /// </summary>
        /// <param name="args">Console args</param>
        /// <returns>Task which resolves a Command.Result</returns>
        public override Task<Result> Main(params string[] args)
        {
            return Task.FromResult<Result>(Result.Ok);
        }
    }
}
