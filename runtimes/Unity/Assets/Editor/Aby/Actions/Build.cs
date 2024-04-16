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
            var sceneName = "Sandbox";
            EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity");
            Debug.LogFormat("Loaded Scene `{0}`", sceneName);
        }
    }
}
