#![allow(unused)]

use std::process::ExitCode;
use std::path::PathBuf;
use std::ffi::CString;

use core::ffi::CStr;

use anyhow::Result;

use cwrap::error::CStringError;

use aby::logging::ffi::CJsRuntimeLogLevel;
use aby::runtime::ffi::CAbyRuntimeConfig;
use aby::runtime::ffi::CExecModuleOptions;

//---
/// TODO
const TRACING_FILTER: &str = "aby-counter=trace,aby=trace,warn";

pub fn try_cstring_from_path(path: PathBuf) -> Result<CString, CStringError> {
    let path_str = path.as_os_str().to_str().unwrap_or_default();
    
    CString::new(path_str).map_err(|error| {
        tracing::error!("Failed to create CString for path '{:}': {:}", path_str, error);
        CStringError::NulError // TODO: Unpack the real error.
    })
}

//---
pub fn main() -> Result<ExitCode> {
    // Step 1:
    //   Setup logging facilities, etc.
    aby::tracing::mount(TRACING_FILTER);
    
    unsafe {
        // Step 2:
        //   Setup runtime config.
        let root_dir = try_cstring_from_path(std::env::current_dir()?)?;
        let main_module_path = CString::new("./examples/counter/main.js")?;
        let db_dir = CString::new("./examples/counter/db")?;
        let log_dir = CString::new("./examples/counter/logs")?;
        let thread_prefix = CString::new("ABY_RUNTIME_COUNTER_EXAMPLE")?;
        let inspector_addr = CString::new("localhost:9222")?;
        
        // Step 3:
        //   Run the bootstrap operation to mount resources and otherwise
        //   prepare the process, ffi boundary, etc.
        let result = aby::runtime::ffi::c_construct_runtime({
            CAbyRuntimeConfig {
                root_dir: root_dir.as_ptr(),
                db_dir: db_dir.as_ptr(),
                main_module_specifier: main_module_path.as_ptr(),
                log_dir: log_dir.as_ptr(),
                log_level: CJsRuntimeLogLevel::Trace,
                log_callback_fn: log_callback,
                inspector_wait: false,
                inspector_addr: inspector_addr.as_ptr(),
            }
        });
        
        // Step 4:
        //   Start the global `MainWorker`. Status 0 is good, 1+ is bad.
        let status = aby::runtime::ffi::c_exec_module(result.runtime, CExecModuleOptions {
            module_specifier: main_module_path.as_ptr(),
        });
        
        tracing::debug!("JsRuntime exited with status {:?}", status);
        
        // Yay! <3
        Ok(ExitCode::SUCCESS)
    }
}

//---
// TODO: Binding generator macro for c-safe functions.
// Turn this:
// #[cwrap::bind(extern "C" aby; panic=true)]
fn wrapped_log_callback(message: &str) {
    tracing::trace!("[Capt'd] {:}", message);
}
// .. into something like this:
extern "C" fn log_callback(level: CJsRuntimeLogLevel, message: *const std::os::raw::c_char) {
    unsafe {
        if !message.is_null() {
            let c_str =  CStr::from_ptr(message);
            let message = c_str.to_str().expect("Failed to unpack log message!");
            wrapped_log_callback(message);
        }
    }
}
