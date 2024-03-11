#![allow(unused)]

use std::process::ExitCode;
use std::ffi::CStr;
use std::ffi::CString;

use anyhow::Result;

use js_runtime::CBootstrapOptions;
use js_runtime::CJsRuntimeConfig;
use js_runtime::CJsRuntimeLogLevel;

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
    
    let thread_prefix = CString::new("TEST").unwrap();
    let main_module_path = CString::new("./examples/main.js").unwrap();
    
    unsafe {
        // Step 1:
        //   Run the bootstrap operation to mount resources and otherwise
        //   prepare the process, ffi boundary, etc.
        js_runtime::bootstrap(CBootstrapOptions {
            int_value: 0,
            thread_prefix: thread_prefix.as_ptr(),
            js_runtime_config: CJsRuntimeConfig {
                main_module_path: main_module_path.as_ptr(),
                log_level: CJsRuntimeLogLevel::Trace,
            }
            
        });
        
        // Step 2:
        //   Mount a log callback for FFI-bound log capture.
        js_runtime::mount_log_callback(log_callback);
        
        // Step 3:
        //   Start the global `MainWorker`. Status 0 is good, 1+ is bad.
        let status = js_runtime::start(0);
        tracing::debug!("JsRuntime exited with status {:?}", status);
        
        // Yay! <3
        Ok(ExitCode::SUCCESS)
    }
}
