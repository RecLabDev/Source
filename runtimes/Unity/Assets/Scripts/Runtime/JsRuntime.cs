using System;
using System.Threading;
using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEditor;

namespace Theta.Unity.Runtime
{
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
        /// <param name="message"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LogCallback(string message);

        /// <summary>
        /// TODO
        /// </summary>
        private static Thread m_ServiceThread;

        /// <summary>
        /// TODO
        /// </summary>
        public static int State => GetState();

        /// <summary>
        /// TODO
        /// </summary>
        public static bool IsAlive => m_ServiceThread.IsAlive;

        /// <summary>
        /// TODO
        /// </summary>
        public static bool IsRunning => m_ServiceThread.ThreadState == ThreadState.Running;

        /// <summary>
        /// TOODO
        /// </summary>
        static JsRuntime()
        {
            try
            {
                // Initialize a thread for the service worker.
                m_ServiceThread = new Thread(new ThreadStart(StartService));

                c_Bootstrap();

                // Mount the logging callback into rust bindings.
                LogCallback callbackDelegate = OnRustLogMessage;
                MountLogCallback(OnRustLogMessage);

                // Keep the callback delcate alive, forever.
                GCHandle.Alloc(callbackDelegate);
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Failed to bootstrap `JsRuntime`: {0}", exception);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="arg"></param>
        [DllImport(AssemblyName, EntryPoint = "js_runtime__bootstrap")]
        private static extern int c_Bootstrap();

        /// <summary>
        /// TODO
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            try
            {
                //..
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Failed to init on load: {0}", exception);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoadBeforeScene()
        {
            try
            {
                //..
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Failed to init runtime on load: {0}", exception);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        [DllImport(AssemblyName, EntryPoint = "js_runtime__mount_log_callback")]
        private static extern int MountLogCallback(LogCallback callback);

        //--
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="arg"></param>
        [DllImport(AssemblyName, EntryPoint = "js_runtime__start", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Start(int arg);

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>Enumerator for co-routine.</returns>
        private static void StartService()
        {
            var exitCode = Start(8080);
            switch (exitCode)
            {
                case 0:
                    Debug.LogFormat("JsRuntime exited safely with code OK ({0}) ..", exitCode);
                    break;
                case 100:
                    Debug.LogWarningFormat("JsRuntime requested shutdown with code RESTART ({0}) ..", exitCode);
                    break;
                default:
                    Debug.LogErrorFormat("JsRuntime exited with code ERROR ({0}).", exitCode);
                    break;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static void StartServiceThread()
        {
            m_ServiceThread.Start();
            Debug.LogFormat("Started Service thread to state {0} ..", State);
        }

        /// <summary>
        /// TODO: This should be an FFI call into the JsRuntime itself.
        /// </summary>
        public static void StopServiceThread()
        {
            try
            {
                // TODO: Safely shutdown JsRuntime ..
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:9000");

                var quitResponse = httpClient.GetStringAsync("/quit");
                if (quitResponse.Result != null)
                {
                    Debug.LogFormat("Called quit endpoint: {0}", quitResponse.Result);
                }
            }
            catch (Exception exc)
            {
                Debug.LogErrorFormat("Couldn't shutdown gracefully: {0}", exc);
            }
            finally
            {
                if (m_ServiceThread.Join(TimeSpan.FromSeconds(5)) == false)
                {
                    Debug.LogError("Failed to join JsRuntime service thread.");
                }

                Debug.LogFormat("Thread State: {0}", m_ServiceThread.ThreadState);
            }
        }

        //---
        /// <summary>
        /// TODO
        /// </summary>
        [DllImport(AssemblyName, EntryPoint = "js_runtime__get_state")]
        private static extern int GetState();

        //--
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="message"></param>
        private static void OnRustLogMessage(string message)
        {
            Debug.LogFormat("[Rust]: {0}", message);
        }
    }
}
