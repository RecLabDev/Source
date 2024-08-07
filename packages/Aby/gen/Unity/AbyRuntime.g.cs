#if !UNITY_WEBGL
// <auto-generated>
// This code is generated by csbindgen.
// DON'T CHANGE THIS DIRECTLY.
// </auto-generated>
#pragma warning disable CS8500
#pragma warning disable CS8981
using System;
using System.Runtime.InteropServices;


namespace Aby.Unity.Plugin
{
    public static unsafe partial class NativeMethods
    {
#if UNITY_IOS && !UNITY_EDITOR
        const string __DllName = "__Internal";
#else
        const string __DllName = "AbyRuntime";
#endif
        



        /// <summary>
        ///  TODO
        /// </summary>
        [DllImport(__DllName, EntryPoint = "aby__verify_log_callback", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void c_verify_log_callback(c_verify_log_callback__cb_delegate _cb);

        /// <summary>
        ///  Construct an instance of AbyRuntime from a c-like boundary.
        ///
        ///  ### Example:
        ///  ```rust
        ///  let result = aby::runtime::ffi::c_construct_runtime({
        ///      CAbyRuntimeConfig {
        ///          // TODO
        ///      }
        ///  });
        ///
        ///  let status = aby::runtime::ffi::c_exec_module(result.runtime, CExecModuleOptions {
        ///      // TODO
        ///  });
        ///  ```
        /// </summary>
        [DllImport(__DllName, EntryPoint = "aby__c_construct_runtime", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern CConstructRuntimeResult c_construct_runtime(CAbyRuntimeConfig c_runtime_config);

        [DllImport(__DllName, EntryPoint = "aby__c_send_broadcast", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void c_send_broadcast(CAbyRuntime* c_aby_runtime_ptr, CSendBroadcastOptions options);

        /// <summary>
        ///  TODO
        /// </summary>
        [DllImport(__DllName, EntryPoint = "aby__c_exec_module", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern CExecModuleResult c_exec_module(CAbyRuntime* c_aby_runtime_ptr, CExecModuleOptions options);

        [DllImport(__DllName, EntryPoint = "aby__c_free_runtime", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void c_free_runtime(CAbyRuntime* c_aby_runtime_ptr);


    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct CAbyRuntime
    {
        public CAbyRuntimeConfig config;
        public CAbyRuntimeStatus status;
        public void* ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct CAbyRuntimeConfig
    {
        public byte* root_dir;
        public byte* main_module_specifier;
        public byte* db_dir;
        public byte* log_dir;
        public CJsRuntimeLogLevel log_level;
        public void* log_callback_fn;
        public byte* inspector_name;
        public byte* inspector_addr;
        [MarshalAs(UnmanagedType.U1)] public bool inspector_wait;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct CConstructRuntimeResult
    {
        public CConstructRuntimeResultCode code;
        public CAbyRuntime* runtime;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct CSendBroadcastOptions
    {
        public sbyte* name;
        public byte* data;
        public nuint length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct CExecModuleOptions
    {
        public byte* module_specifier;
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

    public enum CConstructRuntimeResultCode : uint
    {
        Ok,
        InvalidConfig,
        InvalidDataDir,
        InvalidLogDir,
        InvalidMainModule,
        InvalidBindings,
        FailedSetup,
        FailedOperation,
        FailedBroadcast,
    }

    public enum CExecModuleResult : uint
    {
        Ok,
        RuntimeNul,
        RuntimePanic,
        FailedCreateAsyncRuntime,
        FailedFetchingWorkDirErr,
        DataDirInvalidErr,
        LogDirInvalidErr,
        MainModuleInvalidErr,
        MainModuleUninitializedErr,
        FailedModuleExecErr,
        FailedEventLoopErr,
    }

    public enum CAbyRuntimeStatus : uint
    {
        None = 0,
        Cold,
        Startup,
        Warm,
        Failure,
        Panic,
        Shutdown,
    }


}
#endif