using System.Threading.Tasks;

namespace Theta.SDK.Utils
{
    interface IExecutable
    {
        Task<int> Execute(params string[] args);
    }
}
