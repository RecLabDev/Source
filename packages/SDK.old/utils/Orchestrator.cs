using System.Threading.Tasks;

using Theta.SDK.Utils;

namespace Theta.SDK.Services
{
    /// <summary>
    /// Represents a service cluster orchestrator.
    /// </summary>
    public abstract class Orchestrator
    {
        public abstract Task<int> Start();
    }

    /// <summary>
    /// Orchestrates a Theta service cluster.
    /// </summary>
    public class ThetaOrchestrator
    {
        public Task<Result> Start()
        {
            return Task.FromResult(Result.NoOp);
        }
    }
}
