use std::any::Any;
use std::sync::Arc;
use std::sync::Mutex;
use std::panic::PanicInfo;
use std::ffi::CString;

use crate::runtime::AbyRuntimeError;

#[repr(C)]
#[derive(Clone, Debug)]
pub enum CJsRuntimeLogLevel {
    None = 0,
    Error = 1,
    Warning = 2,
    Info = 3,
    Debug = 4,
    Trace = 5,
}

/// TODO
pub struct SafeLogCallback {
    callback: Arc<Mutex<CLogCallback>>,
}

/// TODO
pub type CLogCallback = extern "C" fn(level: CJsRuntimeLogLevel, message: *const std::ffi::c_char);

/// TODO
#[export_name = "aby__verify_log_callback"]
pub unsafe extern "C" fn c_verify_log_callback(_cb: CLogCallback) {
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
            callback(CJsRuntimeLogLevel::Debug, c_string.as_ptr());
        }
    }
}

//---
#[repr(C)]
pub struct CLogMessage {
    pub body: CString,
}

impl TryFrom<&PanicReport<'_>> for CLogMessage {
    type Error = AbyRuntimeError;
    
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
#[allow(dead_code)] // TODO: Remove this asap.
pub(crate) fn handle_panic(payload: Box<dyn Any + Send>) {
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
