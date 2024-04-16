using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEditor;

using Aby.Unity.Plugin;
using UnityEditor.SceneManagement;

namespace Aby.Unity.Editor.Actions
{
    /// <summary>
    /// TODO
    /// </summary>
    public static class Run
    {
        /// <summary>
        /// TODO
        /// </summary>
        public static void Server()
        {
            if (AbyRuntime.IsRunning)
            {
                Debug.LogWarning("AbyRuntime already running ({0}). Can't run any further!");
            }
            else
            {
                AbyRuntime.StartServiceThread();
                Debug.LogFormat("Running AbyRuntime for Server ({0})", AbyRuntime.State);
            }
        }
    }
}
