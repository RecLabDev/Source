using System;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using UnityEngine;
using static UnityEngine.CullingGroup;

using UnityEditor;
using Aby.Unity.Plugin;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using Aby.SDK;

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
                fixed (byte* execSpecifier = Encoding.UTF8.GetBytes($"{moduleSpecifier}\0"))
                {
                    var runtimeConfig = new CAbyRuntimeConfig
                    {
                        db_dir = dataDir,
                        log_dir = logDir,
                        log_callback_fn = Marshal.GetFunctionPointerForDelegate(m_LogCallback).ToPointer(),
                        inspector_addr = inspectorAddr, // TODO: Get this from args.
                    };

                    Debug.LogFormat(
                        "Executing Module with bootstrap config: Db: {0}; Log: {1}; Module: {2}",
                        GetStringFromBytePtr(runtimeConfig.db_dir),
                        GetStringFromBytePtr(runtimeConfig.log_dir),
                        GetStringFromBytePtr(mainSpecifier)
                    );

                    Debug.Log("TODO: Implement the exec call!");
                    //var runtime = new AbyRuntimeModule(runtimeConfig);
                    //var execOptions = new CExecModuleOptions
                    //{
                    //    module_specifier = execSpecifier,
                    //};

                    //// TODO: Use u16 for the port.
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
