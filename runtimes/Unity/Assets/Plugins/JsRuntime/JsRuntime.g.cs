#if !UNITY_WEBGL
// <auto-generated>
// This code is generated by csbindgen.
// DON'T CHANGE THIS DIRECTLY.
// </auto-generated>
#pragma warning disable CS8500
#pragma warning disable CS8981
using System;
using System.Runtime.InteropServices;


namespace Theta.Unity.Runtime
{
    public static unsafe partial class JsRuntime
    {
#if UNITY_IOS && !UNITY_EDITOR
        const string __DllName = "__Internal";
#else
        const string __DllName = "JsRuntime";
#endif
        



        /// <summary>Initialize a global static `JsRuntime`` instance.  Use this when you want to create a single, managed instance of Deno's `MainWorker` for use in another managed environment.</summary>
        [DllImport(__DllName, EntryPoint = "js_runtime__bootstrap", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern byte bootstrap(CBootstrapOptions options);

        /// <summary>TODO</summary>
        [DllImport(__DllName, EntryPoint = "js_runtime__get_state", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern CJsRuntimeState get_state();

        /// <summary>TODO</summary>
        [DllImport(__DllName, EntryPoint = "js_runtime__mount_log_callback", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern CMountLogResult mount_log_callback(mount_log_callback_log_callback_delegate log_callback);

        /// <summary>TODO: Return a CJsRuntimeStartResult (repr(C)) for state.</summary>
        [DllImport(__DllName, EntryPoint = "js_runtime__start", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern CStartResult start(byte _command);


    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct CBootstrapOptions
    {
        public int int_value;
        public byte* thread_prefix;
        public CJsRuntimeConfig js_runtime_config;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct CJsRuntimeConfig
    {
        public byte* main_module_path;
        public CJsRuntimeLogLevel log_level;
    }


    public enum CJsRuntimeState : uint
    {
        None = 0,
        Cold = 1,
        Startup = 2,
        Warm = 3,
        Failed = 4,
        Panic = 5,
        Shutdown = 6,
    }

    public enum CJsRuntimeLogLevel : uint
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4,
        Trace = 5,
    }

    public enum CMountLogResult : uint
    {
        Ok = 0,
        UnknownError = 1,
        JsRuntimeMissing = 2,
        LogCaptureFailed = 3,
    }

    public enum CStartResult : uint
    {
        Ok = 0,
        Err = 1,
        BindingErr = 2,
        JsRuntimeErr = 3,
    }


}
#endif