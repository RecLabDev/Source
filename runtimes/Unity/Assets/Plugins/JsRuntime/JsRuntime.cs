using System;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using static UnityEngine.CullingGroup;

using UnityEditor;
using static UnityEngine.Application;

namespace Theta.Unity.Runtime
{
    /// <summary>
    /// TODO
    /// </summary>
    [InitializeOnLoad]
    public static partial class JsRuntime
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
        public static bool IsRunning
        {
            get => m_ServiceThread?.ThreadState == ThreadState.Running;
        }

        //---
        /// <summary>
        /// TOODO
        /// </summary>
        static JsRuntime()
        {
            try
            {
                Bootstrap(OnRustLogMessage);

                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                EditorApplication.quitting += OnEditorQuitting;

                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
                AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
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
        public static void StartService()
        {
            m_ServiceThread = new Thread(new ThreadStart(JsRuntime.Start));

            // TODO: Get configuration and pass it to Start.
            m_ServiceThread.Start();

            // TODO: Wait for the thread to become active first ..
            Debug.LogFormat("Started Service thread to state {0} ..", State);
        }

        /// <summary>
        /// TODO: This should be an FFI call into the JsRuntime itself.
        /// </summary>
        public static void StopService()
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
                StopService();
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
                StopService();
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
