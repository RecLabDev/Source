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
        public static Thread ServiceThread { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public static CJsRuntimeState State => get_state();

        /// <summary>
        /// TODO
        /// </summary>
        public static bool IsRunning
        {
            get => ServiceThread?.ThreadState == ThreadState.Running;
        }

        //---
        /// <summary>
        /// TOODO
        /// </summary>
        static JsRuntime()
        {
            try
            {
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

        /// <summary>
        /// TODO
        /// </summary>
        public static string MainModuleSpecifier = "./Examples/Counter/main.js";

        //--
        /// <summary>
        /// TODO
        /// </summary>
        public static void StartServiceThread()
        {
            Debug.Log("Starting Service thread..");

            ServiceThread = new Thread(new ThreadStart(StartServiceThreadBody));
            ServiceThread.Start();

            // TODO: Wait for the thread to become active first ..
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static void StartServiceThreadBody()
        {
            JsRuntime.ExecuteModule(MainModuleSpecifier);
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
                if (ServiceThread.Join(TimeSpan.FromSeconds(10)) == false)
                {
                    Debug.LogError("Failed to join JsRuntime service thread.");
                }
                else
                {
                    Debug.LogFormat("Thread State: {0}", ServiceThread.ThreadState);
                    ServiceThread = null;
                }
            }
        }

        //---
        /// <summary>
        /// TODO
        /// </summary>
        [InitializeOnLoadMethod]
        private unsafe static void OnLoad()
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
    }
}
