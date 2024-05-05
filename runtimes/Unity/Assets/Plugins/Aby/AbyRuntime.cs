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
using System.Threading.Tasks;
using Aby.Unity.Plugin;

namespace Aby.Unity.Plugin
{
    /// <summary>
    /// TODO
    /// </summary>
    [InitializeOnLoad]
    public partial class AbyRuntime
    {
        /// <summary>
        /// TODO
        /// </summary>
        public static Thread ServiceThread { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public static CAbyRuntimeStatus State => CAbyRuntimeStatus.None; // TODO: Get this value from the actual runtime instance.

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
        static AbyRuntime()
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
                Debug.LogErrorFormat("Failed to bootstrap `AbyRuntime`: {0}", exception);
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
            ServiceThread = new Thread(new ThreadStart(StartServiceThreadBody));
            ServiceThread.Start();

            // TODO: Wait for the thread to become active first ..
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static void StartServiceThreadBody()
        {
            AbyRuntime.ExecuteModule(MainModuleSpecifier);
        }

        /// <summary>
        /// TODO: This should be an FFI call into the AbyRuntime itself.
        /// </summary>
        public static void StopServiceThread()
        {
            try
            {
                // TODO: Safely shutdown AbyRuntime ..
                //var httpClient = new HttpClient();
                //httpClient.BaseAddress = new Uri("http://localhost:11000");

                //var quitResponse = httpClient.GetStringAsync("/quit");
                //quitResponse.Wait(); // TODO: Ugh.

                //if (quitResponse.Result == null)
                //{
                //    Debug.LogWarningFormat("Called quit endpoint: {0}", quitResponse);
                //}
            }
            catch (Exception exc)
            {
                Debug.LogErrorFormat("Couldn't shutdown gracefully: {0}", exc);
            }
            finally
            {
                if (ServiceThread.Join(TimeSpan.FromSeconds(10)) == false)
                {
                    Debug.LogError("Failed to join AbyRuntime service thread.");
                }
                else
                {
                    ServiceThread = null;
                }
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private static bool receiveMessages = false;

        /// <summary>
        /// TODO
        /// </summary>
        public static void StartListening()
        {
            Task.Run(ConsumeMessages);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        private async static Task ConsumeMessages()
        {
            receiveMessages = true;
            while (receiveMessages)
            {
                Debug.Log("Checking for channel changes...");

                // Check for changes in the channel here
                // For example, check a queue, a condition, or listen for an event

                // Process any changes found

                // Simulate waiting for a change in the channel. Replace this with your actual change detection logic.
                await Task.Delay(1000); // Waits asynchronously for 1 second
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static void StopListening()
        {
            receiveMessages = false;
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
                Debug.LogFormat("Quitting AbyRuntime service!");
                StopServiceThread();
            }
        }
    }
}
