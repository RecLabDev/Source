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
use crate::state::CJsRuntimeState;

//---
/// TODO
/// 
/// Uses `OnceLock` for lazy init and lock, `Arc` for sharing,
/// and `Mutex` for inner mutability.
pub static JS_RUNTIME_MANAGER: OnceLock<Arc<Mutex<JsRuntimeManager>>> = OnceLock::new();

/// TODO
pub static JS_RUNTIME_STATE: AtomicU32 = AtomicU32::new(CJsRuntimeState::None as u32);

//---
/// TODO
pub type LogCallback = extern "C" fn(message: *const c_char);

//---
/// TODO
#[repr(C)]
pub struct CBootstrapOptions {
    //.
}

//---
/// Initialize a global static `JsRuntime`` instance.
/// 
/// Use this when you want to create a single, managed instance of Deno's
///   `MainWorker` for use in another managed environment.
#[export_name = "js_runtime__bootstrap"]
pub extern "C" fn bootstrap() -> u8 {
    let js_runtime_mgr = JsRuntimeManager::try_new().expect("Unable to get JsRuntimeManager");
    JS_RUNTIME_MANAGER.get_or_init(|| Arc::new(Mutex::new(js_runtime_mgr)));
    
    JS_RUNTIME_STATE.store(CJsRuntimeState::Cold as u32, Ordering::Relaxed);
    
    0 // <3
}

/// TODO
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
pub unsafe extern "C" fn mount_log_callback(log_callback: LogCallback) -> CMountLogResult {
    let Some(js_runtime) = JS_RUNTIME_MANAGER.get() else {
        JS_RUNTIME_STATE.store(CJsRuntimeState::Panic as u32, Ordering::Relaxed);
        return CMountLogResult::JsRuntimeMissing; // </3
    };
    
    
    
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
    
    js_runtime.lock().expect("Couldn't lock JsRuntime ..").set_log_callback(log_callback);
    
    CMountLogResult::Ok // <3
}

#[repr(C)]
pub enum CMountLogResult {
    Ok = 0,
    UnknownError = 1,
    JsRuntimeMissing = 2,
}

/// TODO: Return a CJsRuntimeStartResult (repr(C)) for state.
#[export_name = "js_runtime__start"]
pub unsafe extern "C" fn start(_command: u8) -> u8 {
    let Some(js_runtime) = JS_RUNTIME_MANAGER.get() else {
        JS_RUNTIME_STATE.store(CJsRuntimeState::Panic as u32, Ordering::Relaxed);
        return 1; // </3
    };
    
    match js_runtime.lock().unwrap().start("./examples/main.js") {
        Ok(status) => {
            tracing::debug!("Runtime exited with status {:?}", status);
        }
        Err(error) => {
            tracing::error!("Couldn't start Theta Runtime: {:?}", error);
            return 1; // </3
        }
    }
            
    0 // <3
}
