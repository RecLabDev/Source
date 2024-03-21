using System;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using UnityEngine;
using static UnityEngine.CullingGroup;

using UnityEditor;

namespace Theta.Unity.Runtime
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
    public static unsafe partial class JsRuntime
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
                fixed (byte* mainSpecifier = Encoding.UTF8.GetBytes($"{MainModuleSpecifier}\0"))
                {
                    var jsRuntimeConfig = new JsRuntimeConfig(new CJsRuntimeConfig
                    {
                        db_dir = dataDir,
                        log_dir = logDir,
                        log_callback_fn = Marshal.GetFunctionPointerForDelegate(m_LogCallback).ToPointer(),
                        inspector_port = 9224, // TODO: Get this from args.
                    });

                    Debug.LogFormat(
                        "Executing Module with bootstrap config: Db: {0}; Log: {1}; Module: {2}",
                        GetStringFromBytePtr(jsRuntimeConfig.Instance.db_dir),
                        GetStringFromBytePtr(jsRuntimeConfig.Instance.log_dir),
                        GetStringFromBytePtr(mainSpecifier)
                    );

                    var jsRuntime = new JsRuntime_v2(jsRuntimeConfig);

                    var startOptions = new CExecModuleOptions
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
            c_Instance = JsRuntime.c_construct_runtime(Config.Instance);
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
        public CStartResult Start(CExecModuleOptions options)
        {
            return JsRuntime.c_exec_module(c_Instance, options);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void Dispose()
        {
            JsRuntime.c_free_runtime(c_Instance);
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public unsafe class JsRuntimeConfig
    {
        private CJsRuntimeConfig c_Instance;

        internal CJsRuntimeConfig Instance => c_Instance;

        public uint InspectorPort => c_Instance.inspector_port;

        public JsRuntimeConfig()
        {
            c_Instance = new CJsRuntimeConfig { };
        }

        public JsRuntimeConfig(CJsRuntimeConfig c_config)
        {
            c_Instance = c_config;
        }

        public CJsRuntimeConfig Inspect()
        {
            return c_Instance;
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
    public delegate void c_verify_log_callback__cb_delegate(string message);
}
