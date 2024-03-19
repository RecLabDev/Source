#[cfg(feature="ffi")]
pub mod ffi {
    use std::sync::atomic::Ordering;
    use std::sync::Arc;
    use std::sync::Mutex;
    use std::ffi::CString;
    
    use crate::runtime::ffi::CJsRuntimeConfig;
    use crate::runtime::ffi::CJsRuntimeState;
    use crate::runtime::ffi::JS_RUNTIME_MANAGER;
    use crate::runtime::ffi::JS_RUNTIME_STATE;
    use crate::runtime::JsRuntimeManager;

    //---
    /// TODO
    #[repr(C)]
    #[derive(Debug)]
    pub struct CBootstrapOptions {
        pub thread_prefix: *const std::ffi::c_char,
        pub js_runtime_config: CJsRuntimeConfig,
    }

    #[repr(C)]
    pub enum CBootstrapResult {
        Ok = 0,
        UnknownError = 1,
        JsRuntimeMissing = 2,
        JsRuntimeFailed = 3,
        LogCaptureFailed = 4,
    }

    //---
    /// Initialize a global static `JsRuntime`` instance.
    /// 
    /// Use this when you want to create a single, managed instance of Deno's
    ///   `MainWorker` for use in another managed environment.
    #[allow(unused)]
    #[export_name = "aby__bootstrap"]
    pub extern "C" fn bootstrap(options: CBootstrapOptions) -> CBootstrapResult {
        let mut js_runtime = match JsRuntimeManager::try_new() {
            Ok(js_runtime) => js_runtime,
            Err(error) => return CBootstrapResult::JsRuntimeFailed,
        };
        
        let log_callback = options.js_runtime_config.log_callback_fn;
        js_runtime.set_log_callback(log_callback);

        // Log panics to the supplied log_callback.
        std::panic::set_hook(Box::new(move |panic_info| {
            match CString::new(crate::logging::ffi::unwrap_panic_message(panic_info)) {
                Ok(c_message) => {
                    log_callback(c_message.as_ptr());
                }
                Err(error) => {
                    eprintln!("Failed to unpack panic message: {:}", error);
                }
            }
        }));
        
        if let Err(error) = js_runtime.capture_trace() {
            let c_message = CString::new(format!("Error: {:}", error)).expect("TODO");
            log_callback(c_message.as_ptr());
        }
        
        JS_RUNTIME_MANAGER.get_or_init(|| Arc::new(Mutex::new(js_runtime)));
        
        JS_RUNTIME_STATE.store(CJsRuntimeState::Cold as u32, Ordering::Relaxed);
        
        CBootstrapResult::Ok
    }
}