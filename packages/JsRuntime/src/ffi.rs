use std::any::Any;
use std::ffi::CStr;
use std::panic::PanicInfo;
use std::sync::Arc;
use std::sync::Mutex;
use std::sync::OnceLock;
use std::sync::atomic::AtomicU32;
use std::sync::atomic::Ordering;
// use std::ffi::CStr;
use std::ffi::CString;
use std::os::raw::c_char;

use crate::runtime::JsRuntimeError;
use crate::runtime::JsRuntimeManager;

//---
/// TODO
/// 
/// Uses `OnceLock` for lazy init and lock, `Arc` for sharing,
/// and `Mutex` for inner mutability.
pub(crate) static JS_RUNTIME_MANAGER: OnceLock<Arc<Mutex<JsRuntimeManager>>> = OnceLock::new();

/// TODO
pub(crate) static JS_RUNTIME_STATE: AtomicU32 = AtomicU32::new(CJsRuntimeState::None as u32);
// use std::sync::atomic::AtomicU32;
// use std::sync::atomic::Ordering;

/// Representing the state of the current `JsRuntime`` instance
/// running in the bound process.
/// 
/// Tagged repr(C) for ffi to Unity, Unreal, etc.
#[repr(C)]
pub enum CJsRuntimeState {
    /// No state has been set, yet. Treat this as "uninitialized".
    None = 0,
    
    /// Runtime has been bootstrapped but not yet "warm" (running).
    Cold = 1,
    
    /// The runtime is executing startup operations. Try again next frame.
    Startup = 2,
    
    /// The runtime is working and has had no problems (yet).
    /// Check later for failures, but all good so far!
    Warm = 3,
    
    /// The runtime failed in a predictable way. The host is free to attempt
    /// to recover. Otherwise, shut down gracefully.
    Failed = 4,
    
    /// The runtime encountered an unrecoverable error. The runtime should
    /// shutdown completely before trying again or bad things can happen.
    Panic = 5,
    
    /// The runtime has quit for some reason.
    Shutdown = 6,
}

impl TryFrom<u32> for CJsRuntimeState {
    /// TODO
    type Error = JsRuntimeError;
    
    /// TODO
    fn try_from(value: u32) -> Result<CJsRuntimeState, Self::Error> {
        match value {
            0 => Ok(CJsRuntimeState::Cold),
            1 => Ok(CJsRuntimeState::Startup),
            2 => Ok(CJsRuntimeState::Warm),
            3 => Ok(CJsRuntimeState::Panic),
            4 => Ok(CJsRuntimeState::Shutdown),
            _ => Err(JsRuntimeError::InvalidState(value)),
        }
    }
}

/// TODO
pub(crate) fn set_state(state: CJsRuntimeState) {
    JS_RUNTIME_STATE.store(state as u32, Ordering::Relaxed);
}

/// TODO
#[inline(always)]
#[export_name = "js_runtime__get_state"]
pub extern "C" fn get_state() -> CJsRuntimeState {
    match CJsRuntimeState::try_from(JS_RUNTIME_STATE.load(Ordering::Relaxed)) {
        Ok(state) => state,
        Err(error) => {
            tracing::error!("Couldn't get state: {:?}", error);
            CJsRuntimeState::None
        }
    }
}

//---
/// TODO
#[repr(C)]
#[derive(Debug)]
pub struct CBootstrapOptions {
    pub thread_prefix: *const c_char,
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

#[repr(C)]
#[derive(Debug)]
pub struct CJsRuntimeConfig {
    pub log_dir: *const c_char,
    pub log_level: CJsRuntimeLogLevel,
    pub log_callback_fn: CLogCallback,
}

#[repr(C)]
#[derive(Debug)]
pub enum CJsRuntimeLogLevel {
    None = 0,
    Error = 1,
    Warning = 2,
    Info = 3,
    Debug = 4,
    Trace = 5,
}

//---
/// Initialize a global static `JsRuntime`` instance.
/// 
/// Use this when you want to create a single, managed instance of Deno's
///   `MainWorker` for use in another managed environment.
#[allow(unused)]
#[export_name = "js_runtime__bootstrap"]
pub extern "C" fn bootstrap(options: CBootstrapOptions) -> CBootstrapResult {
    let mut js_runtime = match JsRuntimeManager::try_new() {
        Ok(js_runtime) => js_runtime,
        Err(error) => return CBootstrapResult::JsRuntimeFailed,
    };
    
    let log_callback = options.js_runtime_config.log_callback_fn;
    js_runtime.set_log_callback(log_callback);

    // Log panics to the supplied log_callback.
    std::panic::set_hook(Box::new(move |panic_info| {
        match CString::new(unwrap_panic_message(panic_info)) {
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

//---
#[repr(C)]
#[derive(Debug)]
pub struct CStartOptions {
    pub main_module_specifier: *const c_char,
}

/// TODO
#[repr(C)]
#[derive(Debug)]
pub enum CStartResult {
    Ok = 0,
    Err = 1,
    BindingErr = 2,
    JsRuntimeErr = 3,
}

/// TODO: Return a CJsRuntimeStartResult (repr(C)) for state.
#[export_name = "js_runtime__start"]
pub unsafe extern "C" fn start(options: CStartOptions) -> CStartResult {
    let Some(js_runtime) = JS_RUNTIME_MANAGER.get() else {
        crate::ffi::set_state(CJsRuntimeState::Panic);
        return CStartResult::BindingErr; // </3
    };
    
    let js_runtime = js_runtime.lock().expect("Failed to get lock for JsRuntime!");
    
    crate::ffi::set_state(CJsRuntimeState::Startup);
    
    if options.main_module_specifier.is_null() {
        return CStartResult::JsRuntimeErr;
    }
    
    let c_str = unsafe {
        CStr::from_ptr(options.main_module_specifier).to_str()
    };
    
    let main_module_specifier = match c_str {
        Ok(specifier) => specifier,
        Err(e) => {
            println!("Failed to convert to UTF-8: {}", e);
            return CStartResult::JsRuntimeErr;
        }
    };

    // TODO: Maybe we should be using a panic hook instead?
    // Ref: https://doc.rust-lang.org/std/panic/fn.set_hook.html
    match std::panic::catch_unwind(|| -> Result<u32, JsRuntimeError> {
        Ok(js_runtime.start(main_module_specifier)?)
    }) {
        Ok(exit_result) => match exit_result {
            Ok(exit_status) => {
                js_runtime.send_log(format!("Runtime exited with status {:}", exit_status));
                crate::ffi::set_state(CJsRuntimeState::Shutdown);
                CStartResult::Ok // <3
            }
            Err(error) => match error {
                JsRuntimeError::DenoAnyError(deno_error) => {
                    js_runtime.send_log(format!("Runtime exited with JavaScript error: {:}", deno_error));
                    crate::ffi::set_state(CJsRuntimeState::Shutdown);
                    CStartResult::JsRuntimeErr // </3
                }
                _ => {
                    js_runtime.send_log(format!("Runtime exited with error: {:#?}", error));
                    crate::ffi::set_state(CJsRuntimeState::Panic);
                    CStartResult::BindingErr // </3
                }
            }
        }
        Err(payload) => {
            handle_panic(payload);
            crate::ffi::set_state(CJsRuntimeState::Panic);
            CStartResult::BindingErr // </3
        }
    }
}

/// TODO
pub struct SafeLogCallback {
    callback: Arc<Mutex<CLogCallback>>,
}

/// TODO
pub type CLogCallback = extern "C" fn(message: *const c_char);

/// TODO
#[export_name = "js_runtime__verify_log_callback"]
pub unsafe extern "C" fn verify_log_callback(_cb: CLogCallback) {
    //..
}

impl SafeLogCallback {
    /// TODO
    pub fn new(callback: CLogCallback) -> Self {
        SafeLogCallback {
            callback: Arc::new(Mutex::new(callback)),
        }
    }

    /// TODO
    #[allow(unused_unsafe)]
    pub fn call(&self, message: &str) {
        let c_string = CString::new(message).expect("CString::new failed");
        let callback = self.callback.lock().unwrap();

        unsafe {
            callback(c_string.as_ptr());
        }
    }
}

//---
#[repr(C)]
pub struct CLogMessage {
    pub body: CString,
}

impl TryFrom<&PanicReport<'_>> for CLogMessage {
    type Error = JsRuntimeError;
    
    fn try_from(report: &PanicReport<'_>) -> Result<Self, Self::Error> {
        Ok(CLogMessage {
            body: CString::new(report.message())?,
        })
    }
}

pub struct PanicReport<'info> {
    panic_info: &'info PanicInfo<'info>,
}

impl<'info> From<&'info PanicInfo<'info>> for PanicReport<'info> {
    fn from(panic_info: &'info PanicInfo<'info>) -> Self {
        PanicReport {
            panic_info
        }
    }
}

impl<'info> PanicReport<'info> {
    fn message(&self) -> String {
        format!("JsRuntime Panic: {:#?}", self.panic_info)
    }
}

pub fn unwrap_panic_message(panic_info: &PanicInfo<'_>) -> String {
    let payload = panic_info.payload();
    let location = panic_info.location();

    match payload.downcast_ref::<&str>() {
        Some(s) => format!("Encountered panic at `{:?}`: {}", location, s),
        None => match payload.downcast_ref::<String>() {
            Some(s) => format!("Encountered panic at `{:?}`: {}", location, s),
            None => format!("Encountered unknown panic at `{:?}`: {:?}", location, payload),
        },
    }
}

/// TODO
fn handle_panic(payload: Box<dyn Any + Send>) {
    let panic_message = {
        if let Some(panic_msg) = payload.downcast_ref::<&'static str>() {
            String::from(*panic_msg)
        } else if let Some(panic_msg) = payload.downcast_ref::<String>() {
            panic_msg.to_owned()
        } else {
            format!("Unknown Payload: {:?}", payload)
        }
    };
    
    tracing::error!("JsRuntime failed with panic: {:}", panic_message);
}
