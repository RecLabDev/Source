using System;
using System.Threading.Tasks;

using Theta.SDK.Utils;

namespace Theta.SDK.Commands
{
    /// <summary>
    /// Explore command definition.
    /// </summary>
    public sealed class Explore : Command
    {
        public override string Name => "Explore";
        public override string Verb => "explore";
        public override string Summary => "Explore Theta SDK resources.";
        public override string Usage => $"TODO";

        /// <summary>
        /// Default CTOR.
        /// </summary>
        public Explore()
        { }

        /// <summary>
        /// Cleanup the command.
        /// </summary>
        public void Dispose()
        { }

        /// <summary>
        /// Print list of 
        /// </summary>
        /// <param name="args">Console args</param>
        /// <returns>Task which resolves a Command.Result</returns>
        public override Task<Result> Main(params string[] args)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
