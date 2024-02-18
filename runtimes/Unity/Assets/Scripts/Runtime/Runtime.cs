using System;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEditor;

namespace Theta.Unity.Runtime
{
    /// <summary>
    /// TODO
    /// </summary>
    [CreateAssetMenu(fileName = "New Environment", menuName = "Aby/Environment")]
    public class AbyEnvironment : ScriptableObject
    {
        public int servicePortNumber = 8080;
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class ThetaRuntime
    {
        // The InitializeOnLoadMethod attribute tells Unity to execute this
        // static method when the editor loads.
        [InitializeOnLoadMethod]
        private static void Billboard()
        {
            Debug.LogFormat("Theta SDK Assembly Name: {0}", Theta.SDK.Config.AssemblyName);
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class JsRuntime
    {
        /// <summary>
        /// The assembly name to use when referencing ffi boundaries in C#.
        /// 
        /// TODO: Can we make this private?
        /// </summary>
#if UNITY_IOS
        // Note: Native Plugins' symbols are "__Internal" on iOS.
        public const string AssemblyName = "__Internal";
#else
        // TODO: Get this from the editor (if possible).
        public const string AssemblyName = "JsRuntime";
#endif

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="arg"></param>
        [DllImport(AssemblyName, EntryPoint = "js_runtime__bootstrap")]
        private static extern int Bootstrap();

        /// <summary>
        /// TODO
        /// </summary>
        [DllImport(AssemblyName, EntryPoint = "js_runtime__get_state")]
        public static extern int GetState();

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="arg"></param>
        [DllImport(AssemblyName, EntryPoint = "js_runtime__start", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Start(int arg);

        /// <summary>
        /// TODO: Call "js_runtime__bootstrap" here; start in a ScriptableObject.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            Debug.LogFormat("Bootstrapping JsRuntime ..");
            try
            {
                Bootstrap();
                var jsRuntimeState = GetState();
                // Debug.LogFormat("Bootstrapped Runtime to State: {0}", jsRuntimeState);
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Couldn't Load Scene: {0}", exception);
            }
        }

        /// <summary>
        /// TODO: Call "js_runtime__bootstrap" here; start in a ScriptableObject.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoadBeforeScene()
        {
            Debug.LogFormat("Starting JsRuntime ..");
            try
            {
                // TODO: Ensure JsRuntime is running when the scene starts.
                // var startReport = Start(8080);
                // Debug.LogFormat("JsRuntime exited with result: {0}", startReport);

                var jsRuntimeState = GetState();
                Debug.LogFormat("Got Runtime State: {0}", jsRuntimeState);
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Couldn't Load Scene: {0}", exception);
            }
        }
    }
}
