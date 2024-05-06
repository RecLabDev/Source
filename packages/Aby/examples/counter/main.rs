#![allow(unused)]

use std::process::ExitCode;
use std::path::PathBuf;
use std::ffi::CString;

use core::ffi::CStr;

use aby::runtime::ffi::CConstructRuntimeResultCode;
use anyhow::Result;

use cwrap::error::CStringError;

use aby::tracing::ffi::CJsRuntimeLogLevel;
use aby::runtime::ffi::CAbyRuntimeConfig;
use aby::runtime::ffi::CExecModuleOptions;
use aby::runtime::AbyRuntimeError;

//---
/// TODO: Remove `swc_ecma_codegen` when we've resolved the sourcemap error.
const TRACING_FILTER: &str = "aby-counter=trace,aby=trace,swc_ecma_codegen=off,warn";

/// TODO
pub fn try_path_from_cstring(path: CString) -> Result<PathBuf, CStringError> {
    Ok(std::path::PathBuf::from(cwrap::string::try_unwrap_cstr(path.as_ptr())?))
}

/// TODO
pub fn try_cstring_from_path(path: PathBuf) -> Result<CString, CStringError> {
    let path_str = path.as_os_str().to_str().unwrap_or_default();
    
    CString::new(path_str).map_err(|error| {
        tracing::error!("Failed to create CString for path '{:}': {:}", path_str, error);
        CStringError::NulError // TODO: Unpack the real error.
    })
}

//---
fn main() -> Result<ExitCode> {
    // Step 1:
    //  Setup logging facilities, etc.
    aby::tracing::mount(TRACING_FILTER);
    
    unsafe {
        // Step 2:
        //  Setup runtime configuration. This usually involves creating an
        //  ffi manager which interns strings/function pointers, etc, and
        //  provides a public api for the rest of your code to interact with
        //  it safely and efficiently.
        //  
        //  *Hint: Official runtimes (mostly) do this for you.*
        //  
        //  In Unity, the values for these config items are managed by the
        //  editor/asset database and passed to the `AbyPlugin` when we
        //  setup a new mount context.
        //  
        //  Note: In the future, some of these values will likely probably be
        //  pulled directly from Deno (+other) configs.
        let root_dir = try_cstring_from_path(std::env::current_dir()?)?;
        let main_module_path = CString::new("./examples/counter/main.js")?;
        let db_dir = CString::new("./examples/counter/db")?;
        let log_dir = CString::new("./examples/counter/logs")?;
        let thread_prefix = CString::new("ABY_RUNTIME_COUNTER_EXAMPLE")?;
        let inspector_name = CString::new("Aby Runtime Inspector (Counter)")?;
        let inspector_addr = CString::new("127.0.0.1:9222")?;
        
        // Step 3:
        //  Construct the runtime. Will mount resources and otherwise "warm up"
        //  the runtime for other options (like module execution).
        let result = aby::runtime::ffi::c_construct_runtime({
            CAbyRuntimeConfig {
                root_dir: root_dir.as_ptr(),
                main_module_specifier: main_module_path.as_ptr(),
                db_dir: db_dir.as_ptr(),
                log_dir: log_dir.as_ptr(),
                log_level: CJsRuntimeLogLevel::Trace,
                log_callback_fn: example_log_callback,
                inspector_name: inspector_name.as_ptr(),
                inspector_addr: inspector_addr.as_ptr(),
                inspector_wait: false,
            }
        });
        
        let mut runtime = match result.code {
            CConstructRuntimeResultCode::Ok => result.runtime,
            _ => return Err(anyhow::Error::msg(format!("c_construct_runtime failed (code {:?})", result.code))),
        };
        
        // Step 4:
        //  Start the global `MainWorker`. Status 0 is good, 1+ is bad.
        let status = aby::runtime::ffi::c_exec_module(&mut *runtime, CExecModuleOptions {
            module_specifier: main_module_path.as_ptr(),
        });
        
        tracing::debug!("AbyRuntime exited with status {:?}", status);
        
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
extern "C" fn example_log_callback(level: CJsRuntimeLogLevel, message: *const std::os::raw::c_char) {
    unsafe {
        if !message.is_null() {
            let c_str =  CStr::from_ptr(message);
            let message = c_str.to_str().expect("Failed to unpack log message!");
            wrapped_log_callback(message);
        }
    }
}
