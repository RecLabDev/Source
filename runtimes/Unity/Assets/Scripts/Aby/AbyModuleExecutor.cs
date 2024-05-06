using Aby.Unity.Plugin;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace Aby.Unity
{
    /// <summary>
    /// TODO
    /// </summary>
    public class AbyModuleExecutor : MonoBehaviour
    {
        /// <summary>
        /// TODO
        /// </summary>
        public AbyRuntimeConfig Config { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        private AbyRuntime abyRuntime;


        /// <summary>
        /// TODO
        /// </summary>
        void Start()
        {
            //..
        }

        /// <summary>
        /// TODO
        /// </summary>
        void Update()
        {

        }

        /// <summary>
        /// TODO
        /// </summary>
        private void MountRuntime()
        {
            //var result = NativeMethods.c_construct_runtime(Config.c_instance);
            //if (result.code != CConstructRuntimeResultCode.Ok)
            //{
            //    Debug.LogErrorFormat("Failed to mount AbyRuntime: {0}", result.code);
            //}
            //else
            //{
            //    abyRuntime = result.runtime;
            //}
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void ExecModule()
        {
            // if (abyRuntime == null)
            // {
            //     Debug.LogErrorFormat("Failed to exec module; no runtime available!");
            //     return CExecModuleResult.RuntimeNul;
            // }
            // 
            // return NativeMethods.c_exec_module(abyRuntime.Instance, options);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void Dispose()
        {
            //NativeMethods.c_free_runtime(abyRuntime.Instance);
        }
    }

}
