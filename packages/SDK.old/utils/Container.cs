using System;
using System.Threading.Tasks;

using Theta.SDK.Utils;

namespace Theta.SDK.Services
{
    public abstract class Container : IExecutable
    {
        public Task<int> Execute(params string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
