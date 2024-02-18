using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Theta.Unity.Runtime;

namespace Theta.Unity.Editor.Actions
{
    /// <summary>
    /// Encapsulates static build actions for the Theta Unity runtime.
    /// </summary>
    public static class Build
    {
        /// <summary>
        /// Builds the default Sandbox scene and bootstraps the `JsRuntime`
        /// with a "sandbox" configuration.
        /// </summary>
        public static void Sandbox()
        {
            // string[] defaultScene = { "Assets/Sandbox.unity" };
            // BuildPipeline.BuildPlayer(defaultScene, "Build/windowsGame.exe", BuildTarget.StandaloneWindows, BuildOptions.None);

            Debug.LogFormat("Running Build Sandbox <3");
            Debug.LogFormat("JsRuntime Lib Name: {0}", JsRuntime.AssemblyName);
        }
    }
}
