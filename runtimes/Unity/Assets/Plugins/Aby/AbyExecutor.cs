using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using UnityEngine;
using UnityEditor;

namespace Aby.Unity.Plugin
{
    /// <summary>
    /// TODO
    /// </summary>
    [CreateAssetMenu(fileName = "Aby Runtime Config", menuName = "Aby/Config")]
    public class AbyExecutor : ScriptableObject
    {
        [SerializeField]
        public string RootDir { get; private set; }

        /// <summary>
        /// The most-recently cached configuration data as defined by the
        /// synthetic fields on the current scriptable object instance.
        /// </summary>
        public AbyRuntimeConfig Config { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        private AbyRuntime abyRuntime = null;
    }
}
