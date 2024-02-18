use std::sync::atomic::AtomicU32;
use std::sync::atomic::Ordering;
use std::sync::Arc;
use std::sync::OnceLock;

use crate::runtime::JsRuntimeManager;
use crate::runtime::CJsRuntimeState;

//---
/// TODO
pub static JS_RUNTIME_MANAGER: OnceLock<Arc<JsRuntimeManager>> = OnceLock::new();

/// TODO
pub static JS_RUNTIME_STATE: AtomicU32 = AtomicU32::new(CJsRuntimeState::None as u32);

//---
/// Initialize a global static `JsRuntime`` instance.
/// 
/// Use this when you want to create a single, managed instance of Deno's
///   `MainWorker` for use in another managed environment.
#[export_name = "js_runtime__bootstrap"]
pub extern "C" fn bootstrap() {
    let js_runtime_mgr = JsRuntimeManager::try_new().expect("Unable to get JsRuntimeManager");
    JS_RUNTIME_MANAGER.get_or_init(|| Arc::new(js_runtime_mgr));
    JS_RUNTIME_STATE.store(CJsRuntimeState::Cold as u32, Ordering::Relaxed);
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

/// TODO: Return a CJsRuntimeStartResult (repr(C)) for state.
#[export_name = "js_runtime__start"]
pub unsafe extern "C" fn start(_command: u8) -> u8 {
    let Some(js_runtime) = JS_RUNTIME_MANAGER.get() else {
        JS_RUNTIME_STATE.store(CJsRuntimeState::Panic as u32, Ordering::Relaxed);
        return 1; // </3
    };
    
    match js_runtime.start("./examples/main.js") {
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
