using System;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using UnityEngine;
using static UnityEngine.CullingGroup;

using UnityEditor;
using static UnityEngine.Application;

using Aby.SDK;
using Aby.CStuff;

namespace Aby.Unity.Plugin
{
    /// <summary>
    /// TODO
    /// </summary>
    public partial class AbyRuntime
    {
        /// <summary>
        /// TODO
        /// </summary>
        internal CAbyRuntime c_abyRuntime;

        /// <summary>
        /// TODO
        /// </summary>
        public Thread ServiceThread { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsAlive
        {
            get => ServiceThread?.ThreadState == ThreadState.Running;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public CAbyRuntimeStatus State => CAbyRuntimeStatus.None; // TODO: Get this value from the actual runtime instance.

        /// <summary>
        /// TODO
        /// </summary>
        private LogCallbackDelegate logCallback;

        //--
        /// <summary>
        /// TODO
        /// </summary>
        public void StartServiceThread()
        {
            ServiceThread = new Thread(new ThreadStart(StartServiceThreadBody));
            ServiceThread.Start();

            // TODO: Wait for the thread to become active first ..
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void StartServiceThreadBody()
        {
            //AbyRuntime.ExecuteModule(MainModuleSpecifier);
        }

        /// <summary>
        /// TODO: This should be an FFI call into the AbyRuntime itself.
        /// </summary>
        public void StopServiceThread()
        {
            try
            {
                // TODO: Safely shutdown AbyRuntime ..
                // var httpClient = new HttpClient();
                // httpClient.BaseAddress = new Uri("http://localhost:11000");

                // var quitResponse = httpClient.GetStringAsync("/quit");
                // quitResponse.Wait(); // TODO: Ugh.

                // if (quitResponse.Result == null)
                // {
                //     Debug.LogWarningFormat("Called quit endpoint: {0}", quitResponse);
                // }
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
        /// <param name="message"></param>
        private static void OnAbyLogMessage(string message)
        {
            Debug.LogFormat("[Aby]: {0}", message);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void Dispose()
        {
            // NativeMethods.c_free_runtime(abyRuntime.Instance);
        }

        //---
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="moduleSpecifier"></param>
        public void ExecuteModule(string moduleSpecifier)
        {
            CExecuteModule(moduleSpecifier);
        }

        /// <summary>
        /// TODO: Can/should we merge this into the `ExecuteModule` method above?
        /// </summary>
        /// <returns>Enumerator for co-routine.</returns>
        private unsafe void CExecuteModule(string moduleSpecifier)
        {
            Debug.LogFormat("Executing Module '{0}'", moduleSpecifier);

            try
            {
                if (logCallback == null)
                {
                    logCallback = OnAbyLogMessage;
                    // verify_log_callback(m_LogCallback as verify_log_callback__cb_delegate);
                    // GCHandle.Alloc(logCallback);
                }

                fixed (byte* execSpecifier = CGoodies.GetBytes(moduleSpecifier))
                {
                    var execOptions = new CExecModuleOptions
                    {
                        module_specifier = execSpecifier,
                    };

                    // TODO: Use u16 for the port.
                    //var startResult = runtime.ExecModule(execOptions);
                    //if (startResult != CExecModuleResult.Ok)
                    //{
                    //    throw new Exception($"AbyRuntime exited with error: {startResult}");
                    //}
                    //else
                    //{
                    //    Debug.LogFormat("AbyRuntime exited safely with code OK ({0}) ..", startResult);
                    //}
                }
            }
            catch (Exception exc)
            {
                Debug.LogErrorFormat("Failed to start AbyRuntime: {0}", exc);
            }
        }
    }

    [InitializeOnLoad]
    public partial class AbyEditorExtras
    {
        /// <summary>
        /// TODO
        /// </summary>
        private static bool receiveMessages = false;

        /// <summary>
        /// TOODO
        /// </summary>
        static AbyEditorExtras()
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
        private static void OnBeforeAssemblyReload()
        {
            //if (IsRunning)
            //{
            //    //StopServiceThread();
            //    Debug.LogFormat("Stopped service before assembly reload ..");
            //}
        }

        /// <summary>
        /// TODO
        /// </summary>
        private static void OnAfterAssemblyReload()
        {
            //if (!IsRunning)
            //{
            //    // TODO: Re-start the server, but only if it was running before assembly reload.
            //    // StartServiceThread();
            //    // Debug.LogFormat("Re-started servicec after assembly reload ..");
            //}
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
            // if (IsRunning)
            // {
            //     Debug.LogFormat("Quitting AbyRuntime service!");
            //     StopServiceThread();
            // }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void StartListening()
        {
            Task.Run(ConsumeMessages);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        private async Task ConsumeMessages()
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
    }

    /// <summary>
    /// TODO
    /// </summary>
    public struct AbyRuntimeConfig
    {
        /// <summary>
        /// TODO
        /// </summary>
        public string RootDirectory;

        /// <summary>
        /// TODO
        /// </summary>
        public string DataDirectory;

        /// <summary>
        /// TODO
        /// </summary>
        public string LogDirectory;

        /// <summary>
        /// TODO
        /// </summary>
        public string MainModuleSpecifier;

        /// <summary>
        /// TODO
        /// </summary>
        public string InspectorAddress;
    }

    public unsafe partial struct CAbyRuntimeConfig
    {
        //..
        public string InspectorAddress => CGoodies.PtrToStringUtf8(inspector_addr);
    }

    public static unsafe partial class NativeMethods
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="message"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void c_verify_log_callback__cb_delegate(string message);
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="message"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogCallbackDelegate(string message);
}
