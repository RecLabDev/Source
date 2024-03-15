#![allow(unused)]

use std::process::ExitCode;
use std::ffi::CStr;
use std::ffi::CString;

use anyhow::Result;

use js_runtime::logging::ffi::CJsRuntimeLogLevel;
use js_runtime::runtime::ffi::CJsRuntimeConfig;
use js_runtime::bootstrap::ffi::CBootstrapOptions;
use js_runtime::start::ffi::CStartOptions;

/// TODO
const TRACING_FILTER: &str = "runtime=trace,js_runtime=trace,info";

// Define a function that matches the expected signature for the log callback.
extern "C" fn log_callback(message: *const std::os::raw::c_char) {
    if !message.is_null() {
        let c_str = unsafe { CStr::from_ptr(message) };
        let message = c_str.to_str().expect("Failed to unpack log message!");
        tracing::trace!("[Capt'd] {:}", message);
    }
}

//---
fn main() -> Result<ExitCode> {
    // Setup logging facilities, etc.
    js_runtime::tracing::mount(TRACING_FILTER);
    
    let thread_prefix = CString::new("JS_RUNTIME_EXAMPLE").unwrap();
    let main_module_path = CString::new("./examples/counter/main.js").unwrap();
    let db_dir = CString::new("./examples/counter/db").unwrap();
    let log_dir = CString::new("./examples/counter/logs").unwrap();
    
    unsafe {
        // Step 1:
        //   Run the bootstrap operation to mount resources and otherwise
        //   prepare the process, ffi boundary, etc.
        js_runtime::bootstrap::ffi::bootstrap(CBootstrapOptions {
            thread_prefix: thread_prefix.as_ptr(),
            js_runtime_config: CJsRuntimeConfig {
                db_dir: db_dir.as_ptr(),
                log_dir: log_dir.as_ptr(),
                log_level: CJsRuntimeLogLevel::Trace,
                log_callback_fn: log_callback,
            }
        });
        
        // Step 2:
        //   Start the global `MainWorker`. Status 0 is good, 1+ is bad.
        let status = js_runtime::start::ffi::start(CStartOptions {
            main_module_specifier: main_module_path.as_ptr(),
        });
        
        tracing::debug!("JsRuntime exited with status {:?}", status);
        
        // Yay! <3
        Ok(ExitCode::SUCCESS)
    }
}
