using System;
using System.Threading;
using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEditor;
using static UnityEngine.CullingGroup;
using AOT;
using Unity.XR.Oculus;

namespace Theta.Unity.Runtime
{
    /// <summary>
    /// TODO
    /// </summary>
    [InitializeOnLoad]
    public partial class JsRuntime
    {
        /// <summary>
        /// TODO
        /// </summary>
        private static Thread m_ServiceThread;

        /// <summary>
        /// TODO
        /// </summary>
        public static CJsRuntimeState State => get_state();

        /// <summary>
        /// TODO
        /// </summary>
        public static bool IsAlive => m_ServiceThread.IsAlive;

        /// <summary>
        /// TODO
        /// </summary>
        public static bool IsRunning
        {
            get
            {
                return m_ServiceThread != null
                    && m_ServiceThread.ThreadState == ThreadState.Running;
            }
        }

        //---
        /// <summary>
        /// TOODO
        /// </summary>
        static JsRuntime()
        {
            try
            {
                bootstrap(new CBootstrapOptions { });

                mount_log_callback(OnRustLogMessage);

                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
                AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                EditorApplication.quitting += OnEditorQuitting;
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Failed to bootstrap `JsRuntime`: {0}", exception);
            }
        }

        //--
        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>Enumerator for co-routine.</returns>
        private static void StartService()
        {
            try
            {
                // TODO: Use u16 for the port.
                var exitCode = start(255);
                if (exitCode != CStartResult.Ok)
                {
                    throw new Exception($"JsRuntime exited with exit code ERR ({exitCode})");
                }
                else
                {
                    Debug.LogFormat("JsRuntime exited safely with code OK ({0}) ..", exitCode);
                }
            }
            catch (Exception exc)
            {
                Debug.LogErrorFormat("Failed to start JsRuntime service: {0}", exc);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static void StartServiceThread()
        {
            m_ServiceThread = new Thread(new ThreadStart(StartService));
            m_ServiceThread.Start();

            // TODO: Wait for the thread to become active first ..
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
            catch (HttpRequestException exc)
            {
                Debug.LogFormat("Server shutodown (as expected): {0}", exc);
            }
            catch (AggregateException exc)
            {
                Debug.LogWarningFormat("Server shutodown (as expected): {0}", exc);
            }
            catch (Exception exc)
            {
                Debug.LogErrorFormat("Couldn't shutdown gracefully: {0}", exc);
            }
            finally
            {
                if (m_ServiceThread.Join(TimeSpan.FromSeconds(10)) == false)
                {
                    Debug.LogError("Failed to join JsRuntime service thread.");
                }
                else
                {
                    Debug.LogFormat("Thread State: {0}", m_ServiceThread.ThreadState);
                    m_ServiceThread = null;
                }
            }
        }

        //---
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="message"></param>
        private static void OnRustLogMessage(string message)
        {
            Debug.LogFormat("[Rust]: {0}", message);
        }

        /// <summary>
        /// TODO
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mount_log_callback_log_callback_delegate(string message);

        //---
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
        private static void OnBeforeAssemblyReload()
        {
            if (IsRunning)
            {
                StopServiceThread();
                Debug.LogFormat("Stopped service before assembly reload ..");
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private static void OnAfterAssemblyReload()
        {
            if (!IsRunning)
            {
                // TODO: Re-start the server, but only if it was running before assembly reload.
                // StartServiceThread();
                // Debug.LogFormat("Re-started servicec after assembly reload ..");
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private static void OnEditorQuitting()
        {
            if (IsRunning)
            {
                Debug.LogFormat("Quitting JsRuntime service!");
                StopServiceThread();
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="stateChange"></param>
        private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            Debug.LogFormat("Play mode state changed: {0}", stateChange);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="domainChange"></param>
        private static void OnDomainChanged(object domainChange)
        {
            Debug.LogFormat("Domain reloaded: {0}", domainChange);
        }
    }
}
