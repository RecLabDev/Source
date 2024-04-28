﻿using System;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using UnityEngine;
using static UnityEngine.CullingGroup;

using UnityEditor;
using Aby.Unity.Plugin;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace Aby.Unity.Plugin
{
    // TODO: Move this to the DotNet SDK.
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
    public unsafe partial class AbyRuntime
    {
        /// <summary>
        /// TODO
        /// </summary>
        private static LogCallbackDelegate m_LogCallback;

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>Enumerator for co-routine.</returns>
        private unsafe static void ExecuteModule(string moduleSpecifier)
        {
            try
            {
                if (m_LogCallback == null)
                {
                    m_LogCallback = OnAbyLogMessage;
                    // verify_log_callback(m_LogCallback as verify_log_callback__cb_delegate);
                    // GCHandle.Alloc(logCallback);
                }

                fixed (byte* logDir = Encoding.UTF8.GetBytes("./Logs\0"))
                fixed (byte* dataDir = Encoding.UTF8.GetBytes("./Data/Store\0"))
                fixed (byte* inspectorAddr = Encoding.UTF8.GetBytes("127.0.0.1:9222\0"))
                fixed (byte* mainSpecifier = Encoding.UTF8.GetBytes($"{MainModuleSpecifier}\0"))
                {
                    var jsRuntimeConfig = new JsRuntimeConfig(new CAbyRuntimeConfig
                    {
                        db_dir = dataDir,
                        log_dir = logDir,
                        log_callback_fn = Marshal.GetFunctionPointerForDelegate(m_LogCallback).ToPointer(),
                        inspector_addr = inspectorAddr, // TODO: Get this from args.
                    });

                    Debug.LogFormat(
                        "Executing Module with bootstrap config: Db: {0}; Log: {1}; Module: {2}",
                        GetStringFromBytePtr(jsRuntimeConfig.c_Instance.db_dir),
                        GetStringFromBytePtr(jsRuntimeConfig.c_Instance.log_dir),
                        GetStringFromBytePtr(mainSpecifier)
                    );

                    var jsRuntime = new AbyRuntime_v2(jsRuntimeConfig);

                    var startOptions = new CExecModuleOptions
                    {
                        module_specifier = mainSpecifier,
                    };

                    // TODO: Use u16 for the port.
                    var startResult = jsRuntime.Start(startOptions);
                    if (startResult != CExecModuleResult.Ok)
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

        private unsafe static string GetStringFromBytePtr(byte* ptr)
        {
            if (ptr == null)
            {
                return null;
            }

            // Find the null terminator
            int length = 0;
            while (ptr[length] != 0)
            {
                length++;
            }

            // Convert the bytes to a string
            return new string((sbyte*)ptr, 0, length, Encoding.UTF8);
        }

        //---
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="message"></param>
        private static void OnAbyLogMessage(string message)
        {
            Debug.LogFormat("[Aby]: {0}", message);
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public unsafe class AbyRuntime_v2 : IDisposable
    {
        /// <summary>
        /// TODO
        /// </summary>
        private CAbyRuntime* c_Instance;

        /// <summary>
        /// TODO
        /// </summary>
        public JsRuntimeConfig Config { get; private set; }

        /// <summary>
        /// TODO
        /// </summary>
        public AbyRuntime_v2(JsRuntimeConfig config)
        {
            var result = NativeMethods.c_construct_runtime(config.c_Instance);
            if (result.code != CConstructRuntimeResultCode.Ok)
            {
                Debug.LogErrorFormat("Failed to mount AbyRuntime: {0}", result.code);
            }
            else
            {
                c_Instance = result.runtime;
                Config = config;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public AbyRuntime_v2(CAbyRuntimeConfig config)
        {
            Config = new JsRuntimeConfig(config);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public CExecModuleResult Start(CExecModuleOptions options)
        {
            return NativeMethods.c_exec_module(c_Instance, options);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void Dispose()
        {
            NativeMethods.c_free_runtime(c_Instance);
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public unsafe class JsRuntimeConfig
    {
        public string InspectorAddress => c_Instance.InspectorAddress;

        internal CAbyRuntimeConfig c_Instance;

        public JsRuntimeConfig()
        {
            c_Instance = new CAbyRuntimeConfig { };
        }

        public JsRuntimeConfig(CAbyRuntimeConfig c_config)
        {
            c_Instance = c_config;
        }

        public CAbyRuntimeConfig Inspect()
        {
            return c_Instance;
        }
    }

    public unsafe partial struct CAbyRuntimeConfig
    {
        //..
        public string InspectorAddress => NativeMethods.PtrToStringUtf8(inspector_addr);
    }

    public static unsafe partial class NativeMethods
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="message"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void c_verify_log_callback__cb_delegate(string message);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static unsafe string PtrToStringUtf8(byte* ptr)
        {
            if (ptr == null)
            {
                return null;
            }

            byte* temp = ptr;
            while (*temp != 0)
            {
                temp++;
            }

            int length = (int)(temp - ptr);

            // Create a string from the byte array
            return Encoding.UTF8.GetString(ptr, length);
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="message"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogCallbackDelegate(string message);
}
