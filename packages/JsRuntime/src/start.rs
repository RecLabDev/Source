
//---
#[cfg(feature="ffi")]
pub mod ffi {
    use std::ffi::CStr;
    
    use crate::runtime::JsRuntimeError;
    
    use crate::runtime::ffi::CJsRuntimeState;
    use crate::runtime::ffi::JS_RUNTIME_MANAGER;
    
    //---
    #[repr(C)]
    #[derive(Debug)]
    pub struct CStartOptions {
        pub main_module_specifier: *const std::ffi::c_char,
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
            crate::runtime::ffi::set_state(CJsRuntimeState::Panic);
            return CStartResult::BindingErr; // </3
        };
        
        let js_runtime = js_runtime.lock().expect("Failed to get lock for JsRuntime!");
        
        crate::runtime::ffi::set_state(CJsRuntimeState::Startup);
        
        
        
        let c_str = if options.main_module_specifier.is_null() {
            return CStartResult::JsRuntimeErr;
        } else {
            CStr::from_ptr(options.main_module_specifier)
        };
        
        let main_module_specifier = match c_str.to_str() {
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
                    crate::runtime::ffi::set_state(CJsRuntimeState::Shutdown);
                    CStartResult::Ok // <3
                }
                Err(error) => match error {
                    JsRuntimeError::DenoAnyError(deno_error) => {
                        js_runtime.send_log(format!("Runtime exited with JavaScript error: {:}", deno_error));
                        crate::runtime::ffi::set_state(CJsRuntimeState::Shutdown);
                        CStartResult::JsRuntimeErr // </3
                    }
                    _ => {
                        js_runtime.send_log(format!("Runtime exited with error: {:#?}", error));
                        crate::runtime::ffi::set_state(CJsRuntimeState::Panic);
                        CStartResult::BindingErr // </3
                    }
                }
            }
            Err(payload) => {
                crate::logging::ffi::handle_panic(payload);
                crate::runtime::ffi::set_state(CJsRuntimeState::Panic);
                CStartResult::BindingErr // </3
            }
        }
    }
}
