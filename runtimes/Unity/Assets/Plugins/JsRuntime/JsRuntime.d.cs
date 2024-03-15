using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using UnityEngine;
using static UnityEngine.CullingGroup;

using UnityEditor;
using System.Collections.Concurrent;

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
        public static string MainModuleSpecifier = "./Examples/Counter/main.js";

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>Enumerator for co-routine.</returns>
        private unsafe static void Start()
        {
            try
            {
                Bootstrap();

                fixed (byte* specifier = Encoding.UTF8.GetBytes(MainModuleSpecifier))
                {
                    var startOptions = new CStartOptions
                    {
                        main_module_specifier = specifier,
                    };

                    // TODO: Use u16 for the port.
                    var startResult = start(startOptions);
                    if (startResult != CStartResult.Ok)
                    {
                        throw new Exception($"JsRuntime exited with exit code ERR ({startResult})");
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
