#![allow(unused)]

use std::error::Error;
use std::fmt::Display;
use std::sync::atomic::Ordering;
use std::rc::Rc;
use std::sync::Arc;

use deno_runtime::deno_web::BlobStore;
use deno_runtime::permissions::PermissionsContainer;

use deno_runtime::deno_core::JsRuntime;
use deno_runtime::deno_core::RuntimeOptions;
use deno_runtime::deno_core::FsModuleLoader;
use deno_runtime::deno_core::PollEventLoopOptions;
use deno_runtime::deno_core::ModuleResolutionError;
use deno_runtime::deno_core::resolve_url_or_path;

use deno_runtime::worker::MainWorker;
use deno_runtime::worker::WorkerOptions;

use tokio::runtime::Runtime as TokioRuntime;
use tokio::runtime::Builder as TokioRuntimeBuilder;

#[cfg(feature="ffi")]
use crate::ffi::JS_RUNTIME_STATE;

//---
/// TODO
pub struct JsRuntimeManager {
    /// TODO
    async_runtime: TokioRuntime,
}

impl JsRuntimeManager {
    /// TODO
    pub fn try_new() -> Result<JsRuntimeManager, std::io::Error> {
        let async_runtime = TokioRuntimeBuilder::new_current_thread()
            .enable_time()
            .enable_io()
            .build()?;
        
        Ok(JsRuntimeManager {
            async_runtime,
        })
    }
}

impl JsRuntimeManager {
    /// TODO
    pub fn start(&self, main_specifier: &str) -> Result<u32, JsRuntimeError> {
        // TODO: Move this to bootstrap methods.
        let current_dir = std::env::current_dir()?;
        
        #[cfg(feature = "verbose")]
        tracing::debug!("Executing in {:}", current_dir.display());
        
        // TODO: Move this to `ThetaRuntime::resolve_main_module(..)`.
        let main_module = resolve_url_or_path(main_specifier, &current_dir)?;
        
        // Run the "lite" Deno runtime, with only a core.
        // Useful for some testing scenarios.
        #[cfg(feature = "lite")]
        self.async_runtime.block_on(async move {
            let mut js_runtime = JsRuntime::new(RuntimeOptions {
                module_loader: Some(Rc::new(FsModuleLoader)),
                extensions: vec![
                    // deno_runtime::deno_webidl::deno_webidl::init_ops_and_esm(),
                    // deno_runtime::deno_console::deno_console::init_ops_and_esm(),
                    // deno_runtime::deno_url::deno_url::init_ops_and_esm(),
                    // deno_runtime::deno_web::deno_web::init_ops_and_esm::<PermissionsContainer>(Arc::new(BlobStore::default()), None),
                ],
                ..Default::default()
            });
            
            if let Err(error) = js_runtime.execute_script_static("<prelude>", include_str!("./prelude.js")) {
                tracing::error!("Failed to run main worker event loop: {:}", error);
            }
            
            if let Err(error) = js_runtime.execute_script_static("<debug>", include_str!("./debug.js")) {
                tracing::error!("Failed to run main worker event loop: {:}", error);
            }
            
            if let Err(error) = js_runtime.run_event_loop(PollEventLoopOptions::default()).await {
                tracing::error!("Failed to run main worker event loop: {:}", error);
            }
        });
        
        // Run the "not-lite", full Deno runtime.
        // Prefer this when you want all of Deno's features.
        #[cfg(not(feature = "lite"))]
        self.async_runtime.block_on(async move {
            let mut js_runtime = MainWorker::bootstrap_from_options(
                // TODO: Revist the Clone for `main_module`.
                main_module.clone(),
                PermissionsContainer::allow_all(),
                WorkerOptions {
                    module_loader: Rc::new(FsModuleLoader),
                    extensions: vec![
                        //..
                    ],
                    ..Default::default()
                },
            );
            
            // TODO: Revist the Clone for `main_module`.
            if let Err(error) = js_runtime.execute_main_module(&main_module.clone()).await {
                tracing::error!("Failed to execute main module: {:}", error);
            }
            
            if let Err(error) = js_runtime.run_event_loop(true).await {
                tracing::error!("Failed to run main worker event loop: {:}", error);
            }
        });
            
        #[cfg(feature="ffi")]
        JS_RUNTIME_STATE.store(CJsRuntimeState::Shutdown as u32, Ordering::Relaxed);
    
        Ok(0)
    }
}

/// TODO
#[derive(Debug)]
pub enum JsRuntimeError {
    /// A user-supplied module-name was invalid.
    InvalidModuleSpecifier(&'static str),
    
    /// The runtime detected a current or future invalid atomic state.
    InvalidState(u32),
    
    /// TODO
    ResourceError(&'static str, std::io::Error),
    
    /// TODO
    ModuleError(ModuleResolutionError),
    
    /// An unknown error occurred.
    Unknown(&'static str),
}

impl Error for JsRuntimeError {}

impl Display for JsRuntimeError {
    fn fmt(&self, mut f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_str("TODO")
    }
}

impl From<std::io::Error> for JsRuntimeError {
    fn from(error: std::io::Error) -> JsRuntimeError {
        JsRuntimeError::ResourceError("io", error)
    }
}

impl From<ModuleResolutionError> for JsRuntimeError {
    fn from(error: ModuleResolutionError) -> JsRuntimeError {
        JsRuntimeError::ModuleError(error)
    }
}

/// Representing the state of the current `JsRuntime`` instance
/// running in the bound process.
/// 
/// Tagged repr(C) for ffi to Unity, Unreal, etc.
#[repr(C)]
pub enum CJsRuntimeState {
    /// No state has been set, yet. Treat this as "uninitialized".
    None = -1,
    
    /// Runtime has been bootstrapped but not yet "warm" (running).
    Cold = 0,
    
    /// The runtime is executing startup operations. Try again next frame.
    Startup = 1,
    
    /// The runtime is working and has had no problems (yet).
    /// Check later for failures, but all good so far!
    Warm = 2,
    
    /// The runtime has identified an error and has initiated shutdown.
    /// Assume the runtime needs to shutdown completely before trying again.
    Panic = 3,
    
    /// The runtime has quit for some reason.
    Shutdown = 4,
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
#[repr(C)]
pub enum CJsRuntimeEventKind {
    Hup = 0,
}
