﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using UnityEngine;
using static UnityEngine.CullingGroup;

using UnityEditor;
using System.Collections.Concurrent;
using System.Xml.Linq;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace Theta.Unity.Runtime
{
    //public class ConcurrentStringInterner : IDisposable
    //{
    //    private ConcurrentDictionary<string, (string, GCHandle)> m_strings = new ConcurrentDictionary<string, (string, GCHandle)>();

    //    // Interns a string and returns its byte pointer
    //    public unsafe byte* InternAndReturnPointer(string value)
    //    {
    //        if (value == null) throw new ArgumentNullException(nameof(value));

    //        var interned = m_strings.GetOrAdd(value, key =>
    //        {
    //            var bytes = Encoding.UTF8.GetBytes(key + "\0"); // Null-terminate for C compatibility
    //            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
    //            return (key, handle);
    //        });

    //        return (byte*)interned.Item2.AddrOfPinnedObject();
    //    }

    //    // Retrieves a byte pointer for an already interned string
    //    public unsafe byte* GetPointer(string value)
    //    {
    //        if (value == null) throw new ArgumentNullException(nameof(value));
    //        if (m_strings.TryGetValue(value, out var tuple))
    //        {
    //            return (byte*)tuple.Item2.AddrOfPinnedObject();
    //        }
    //        throw new InvalidOperationException("String not interned");
    //    }

    //    public void Dispose()
    //    {
    //        foreach (var entry in m_strings.Values)
    //        {
    //            entry.Item2.Free(); // Free the pinned GCHandle
    //        }
    //        m_strings.Clear();
    //        m_strings = null;
    //    }
    //}

    /// <summary>
    /// TODO
    /// </summary>
    public static unsafe partial class JsRuntime
    {
        /// <summary>
        /// TODO
        /// </summary>
        private static LogCallbackDelegate m_LogCallback;

        /// <summary>
        /// TODO
        /// </summary>
        private unsafe static void Bootstrap()
        {
            try
            {
                if (m_LogCallback == null)
                {
                    m_LogCallback = OnRustLogMessage;
                    //verify_log_callback(logCallback as verify_log_callback__cb_delegate);
                    //GCHandle.Alloc(logCallback);
                }

                fixed (byte* logDir = Encoding.UTF8.GetBytes("./Logs"))
                {
                    var bootstrapOptions = new CBootstrapOptions
                    {
                        js_runtime_config = new CJsRuntimeConfig
                        {
                            log_dir = logDir,
                            log_callback_fn = (void*)Marshal.GetFunctionPointerForDelegate(m_LogCallback),
                        }
                    };

                    var bootstrapResult = bootstrap(bootstrapOptions);
                    if (bootstrapResult != CBootstrapResult.Ok)
                    {
                        throw new Exception($"JsRuntime exited with exit code ERR ({bootstrapResult})");
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.LogErrorFormat("Failed to bootstrap JsRuntime: {0}", exc);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>Enumerator for co-routine.</returns>
        private unsafe static void ExecuteModule(string mainModuleSpecifier)
        {
            try
            {
                fixed (byte* logDir = Encoding.UTF8.GetBytes("./Logs"))
                fixed (byte* dataDir = Encoding.UTF8.GetBytes("./Data/Store"))
                fixed (byte* mainSpecifier = Encoding.UTF8.GetBytes(MainModuleSpecifier))
                {
                    var jsRuntimeConfig = new JsRuntimeConfig(new CJsRuntimeConfig
                    {
                        db_dir = dataDir,
                        log_dir = logDir,
                    });

                    // Bootstrap();

                    var jsRuntime = new JsRuntime_v2(jsRuntimeConfig);

                    var startOptions = new CExecuteModuleOptions
                    {
                        main_module_specifier = mainSpecifier,
                    };

                    // TODO: Use u16 for the port.
                    var startResult = jsRuntime.Start(startOptions);
                    if (startResult != CStartResult.Ok)
                    {
                        throw new Exception($"JsRuntime exited with error: {startResult}");
                    }
                    else
                    {
                        Debug.LogFormat("JsRuntime exited safely with code OK ({0}) ..", startResult);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.LogErrorFormat("Failed to start JsRuntime: {0}", exc);
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
    }

    /// <summary>
    /// TODO
    /// </summary>
    public unsafe class JsRuntime_v2 : IDisposable
    {
        /// <summary>
        /// TODO
        /// </summary>
        private CJsRuntime* c_Instance;

        /// <summary>
        /// TODO
        /// </summary>
        public JsRuntimeConfig Config { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public JsRuntime_v2(JsRuntimeConfig jsRuntimeConfig)
        {
            Config = jsRuntimeConfig;
            c_Instance = JsRuntime.construct_runtime(Config.Instance);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public JsRuntime_v2(CJsRuntimeConfig config)
        {
            Config = new JsRuntimeConfig(config);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public CStartResult Start(CExecuteModuleOptions options)
        {
            return JsRuntime.execute_module(c_Instance, options);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void Dispose()
        {
            JsRuntime.free_my_object(c_Instance);
        }
    }

    public unsafe class JsRuntimeConfig
    {
        private CJsRuntimeConfig c_Instance;

        internal CJsRuntimeConfig Instance => c_Instance;

        public uint InspectorPort => c_Instance.inspect_port;

        public JsRuntimeConfig()
        {
            c_Instance = new CJsRuntimeConfig { };
        }

        public JsRuntimeConfig(CJsRuntimeConfig c_config)
        {
            c_Instance = c_config;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="message"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogCallbackDelegate(string message);

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="message"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void verify_log_callback__cb_delegate(string message);
}
