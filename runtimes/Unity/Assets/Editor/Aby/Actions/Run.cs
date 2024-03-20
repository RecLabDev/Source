using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEditor;

using Theta.Unity.Runtime;
using UnityEditor.SceneManagement;

namespace Theta.Unity.Editor.Aby.Actions
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
            if (JsRuntime.IsRunning)
            {
                Debug.LogWarning("AbyRuntime already running ({0}). Can't run any further!");
            }
            else
            {
                JsRuntime.StartServiceThread();
                Debug.LogFormat("Running AbyRuntime for Server ({0})", JsRuntime.State);
            }
        }
    }
}
