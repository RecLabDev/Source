use std::any::Any;
use std::panic::PanicInfo;
use std::sync::Arc;
use std::sync::Mutex;
use std::sync::OnceLock;
use std::sync::atomic::AtomicU32;
use std::sync::atomic::Ordering;
// use std::ffi::CStr;
use std::ffi::CString;
use std::os::raw::c_int;
use std::os::raw::c_char;

use crate::runtime::JsRuntimeError;
use crate::runtime::JsRuntimeManager;
use crate::state::CJsRuntimeState;

//---
/// TODO
/// 
/// Uses `OnceLock` for lazy init and lock, `Arc` for sharing,
/// and `Mutex` for inner mutability.
pub(crate) static JS_RUNTIME_MANAGER: OnceLock<Arc<Mutex<JsRuntimeManager>>> = OnceLock::new();

/// TODO
pub(crate) static JS_RUNTIME_STATE: AtomicU32 = AtomicU32::new(CJsRuntimeState::None as u32);

/// TODO
pub(crate) fn set_state(state: CJsRuntimeState) {
    JS_RUNTIME_STATE.store(state as u32, Ordering::Relaxed);
}

//---
/// TODO
pub type CLogCallback = extern "C" fn(message: *const c_char);

/// TODO
pub struct SafeLogCallback {
    callback: Arc<Mutex<CLogCallback>>,
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
/// TODO
#[repr(C)]
#[derive(Debug)]
pub struct CBootstrapOptions {
    pub int_value: c_int,
    pub thread_prefix: *const c_char,
    // pub array_value: *const c_int,
    pub js_runtime_config: CJsRuntimeConfig,
    // pub log_callback_fn: CLogCallback,
}

impl CBootstrapOptions {
    pub fn new() -> Self {
        CBootstrapOptions {
            int_value: 0,
            thread_prefix: CString::default().as_ptr(),
            js_runtime_config: CJsRuntimeConfig {
                main_module_path: CString::default().as_ptr(),
                log_level: CJsRuntimeLogLevel::Trace,
            }
            
        }
    }
}

#[repr(C)]
#[derive(Debug)]
pub struct CJsRuntimeConfig {
    pub main_module_path: *const c_char,
    pub log_level: CJsRuntimeLogLevel,
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
pub extern "C" fn bootstrap(options: CBootstrapOptions) -> u8 {
    let js_runtime_mgr = JsRuntimeManager::try_new().expect("Unable to get JsRuntimeManager");
    JS_RUNTIME_MANAGER.get_or_init(|| Arc::new(Mutex::new(js_runtime_mgr)));
    
    JS_RUNTIME_STATE.store(CJsRuntimeState::Cold as u32, Ordering::Relaxed);
    
    0 // <3
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

/// TODO
#[export_name = "js_runtime__mount_log_callback"]
pub unsafe extern "C" fn mount_log_callback(log_callback: CLogCallback) -> CMountLogResult {
    let Some(js_runtime) = JS_RUNTIME_MANAGER.get() else {
        JS_RUNTIME_STATE.store(CJsRuntimeState::Panic as u32, Ordering::Relaxed);
        return CMountLogResult::JsRuntimeMissing; // </3
    };
    
    let mut js_runtime = js_runtime.lock().expect("Failed to get lock for JsRuntime!");
    
    // Log panics to the supplied log_callback.
    std::panic::set_hook(Box::new(move |panic_info| {
        // let js_runtime = js_runtime.lock().unwrap();
        let location = panic_info.location();
        let payload = panic_info.payload();
        
        let message = match payload.downcast_ref::<&str>() {
            Some(s) => format!("Encountered panic at `{:?}`: {}", location, s),
            None => match payload.downcast_ref::<String>() {
                Some(s) => format!("Encountered panic at `{:?}`: {}", location, s),
                None => format!("Encountered unknown panic at `{:?}`: {:?}", location, payload),
            },
        };
        
        let c_message = CString::new(message).expect("CString::new failed");
        
        log_callback(c_message.as_ptr());
    }));
    
    js_runtime.set_log_callback(log_callback);
    
    js_runtime.capture_trace().expect("Failed to capture trace!");
    
    CMountLogResult::Ok // <3
}

#[repr(C)]
pub enum CMountLogResult {
    Ok = 0,
    UnknownError = 1,
    JsRuntimeMissing = 2,
}

use deno_runtime::deno_core::error::JsError;
use deno_runtime::deno_core::anyhow::Error as DenoAnyError;

/// TODO: Return a CJsRuntimeStartResult (repr(C)) for state.
#[export_name = "js_runtime__start"]
pub unsafe extern "C" fn start(_command: u8) -> CStartResult {
    let Some(js_runtime) = JS_RUNTIME_MANAGER.get() else {
        crate::ffi::set_state(CJsRuntimeState::Panic);
        return CStartResult::BindingPanic; // </3
    };
    
    let js_runtime = js_runtime.lock().expect("Failed to get lock for JsRuntime!");
    
    crate::ffi::set_state(CJsRuntimeState::Startup);
    
    // TODO: Maybe we should be using a panic hook instead?
    // Ref: https://doc.rust-lang.org/std/panic/fn.set_hook.html
    match std::panic::catch_unwind(|| -> Result<u32, JsRuntimeError> {
        Ok(js_runtime.start("./examples/main.js")?)
    }) {
        Ok(exit_result) => match exit_result {
            Ok(exit_status) => {
                tracing::debug!("Runtime exited with status {:}", exit_status);
                crate::ffi::set_state(CJsRuntimeState::Shutdown);
                CStartResult::Ok // <3
            }
            Err(error) => match error {
                JsRuntimeError::DenoAnyError(deno_error) => {
                    tracing::error!("Runtime exited with JavaScript error: {:}", deno_error);
                    crate::ffi::set_state(CJsRuntimeState::Shutdown);
                    CStartResult::JsRuntimeError // </3
                }
                _ => {
                    tracing::error!("Runtime exited with error: {:#?}", error);
                    crate::ffi::set_state(CJsRuntimeState::Panic);
                    CStartResult::BindingError // </3
                }
            }
        }
        Err(payload) => {
            handle_panic(payload);
            crate::ffi::set_state(CJsRuntimeState::Panic);
            CStartResult::BindingPanic // </3
        }
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

/// TODO
#[repr(C)]
#[derive(Debug)]
pub enum CStartResult {
    Ok = 0,
    BindingError = 1,
    BindingPanic = 2,
    JsRuntimeError = 3,
}
