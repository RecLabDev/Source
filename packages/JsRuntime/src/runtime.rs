use std::net::Ipv4Addr;
use std::net::SocketAddr;
use std::net::SocketAddrV4;
use std::time::Duration;
use std::rc::Rc;
use std::sync::Arc;
use std::sync::Mutex;
use std::sync::PoisonError;
use std::sync::MutexGuard;
use std::error::Error;
use std::fmt::Display;
use std::ffi::CString;

#[cfg(not(feature = "stdio"))]
use std::fs::OpenOptions;

use deno_runtime::inspector_server::InspectorServer;
use tokio::runtime::Builder as TokioRuntimeBuilder;
use tokio::runtime::Runtime as TokioRuntime;
use tokio::sync::Mutex as TokioMutex;

use deno_runtime::permissions::PermissionsContainer;

use deno_runtime::deno_core::resolve_url_or_path;
use deno_runtime::deno_core::FeatureChecker;
use deno_runtime::deno_core::FsModuleLoader;
use deno_runtime::deno_core::ModuleResolutionError;
use deno_runtime::deno_core::PollEventLoopOptions;

use deno_runtime::worker::MainWorker;
use deno_runtime::worker::WorkerOptions;

use deno_runtime::BootstrapOptions;
use deno_runtime::UNSTABLE_GRANULAR_FLAGS;
use tokio::task::JoinHandle;

use crate::stdio::JsRuntimeStdio;

#[cfg(feature = "ffi")]
use crate::logging::ffi::CLogCallback;

#[allow(unused)] // TODO
pub struct JsRuntimeConfig {
    db_dir: Option<String>,
    log_dir: Option<String>,
}

impl JsRuntimeConfig {
    pub fn new() -> Self {
        JsRuntimeConfig {
            db_dir: None,
            log_dir: None,
        }
    }
}

//---
/// TODO
#[allow(unused)] // TODO
pub struct JsRuntimeManager {
    /// TODO
    config: JsRuntimeConfig,
    
    /// TODO
    async_runtime: TokioRuntime,

    /// TODO
    stdio: JsRuntimeStdio,

    /// TODO
    /// 1, BroadcastChannel
    /// 2, Deno.cron
    /// 3, FFI
    /// 4, File System
    /// 5, HTTP
    /// 6, Key-Value
    /// 7, Net
    /// 8, Temporal
    /// 9, Proto
    /// 10, WebGPU
    /// 11, Web Worker
    unstable_features: Vec<i32>,

    /// TODO
    log_callback: Option<Arc<Mutex<CLogCallback>>>,

    /// TODO
    log_callback_async: Option<Arc<TokioMutex<CLogCallback>>>,
}

impl JsRuntimeManager {
    /// TODO
    pub fn try_new() -> Result<Self, std::io::Error> {
        let config = JsRuntimeConfig::new();
        
        // If the `stdio` feature is enabled, just use default stdio setup.
        #[cfg(feature = "stdio")]
        let js_stdio = JsRuntimeStdio::try_new(None, None)?;

        // Otherwise, re-route stdin, stdout, and stderr to temporary log files.
        #[cfg(not(feature = "stdio"))]
        let js_stdio = {
            tracing::info!("Feature `stdio` not enabled; Re-routing stdio to logs.");

            JsRuntimeStdio::try_new(
                Some(
                    OpenOptions::new()
                        .read(true)
                        .write(true)
                        .create(true)
                        .open("./Logs/JsRuntime.out.log")?,
                ),
                Some(
                    OpenOptions::new()
                        .read(true)
                        .write(true)
                        .create(true)
                        .open("./Logs/JsRuntime.err.log")?,
                ),
            )?
        };

        // We don't need a TokioRuntime if anything else fails (so we create it last).
        let async_runtime = TokioRuntimeBuilder::new_current_thread()
            .enable_time()
            .enable_io()
            .build()?;

        let unstable_features = UNSTABLE_GRANULAR_FLAGS
            .iter()
            .map(|&feature| feature.2)
            .collect();

        Ok(JsRuntimeManager {
            config,
            async_runtime,
            stdio: js_stdio,
            unstable_features,
            log_callback: None,
            log_callback_async: None,
        })
    }

    /// TODO1
    pub fn with_log_callback(mut self, log_callback: CLogCallback) {
        self.set_log_callback(log_callback)
    }

    /// TODO
    pub fn set_log_callback(&mut self, log_callback: CLogCallback) {
        self.log_callback = Some(Arc::new(Mutex::new(log_callback)));
        self.log_callback_async = Some(Arc::new(TokioMutex::new(log_callback)));
    }
}

#[allow(unused)]
impl JsRuntimeManager {
    /// TODO
    pub fn capture_trace(&self) -> Result<JoinHandle<u8>, JsRuntimeError> {
        let log_callback = self.log_callback_async.as_ref().ok_or(JsRuntimeError::LogCallbackMissing)?;
        
        // TODO: We shouldn't be cloning here. Find a way to share the data more safely.
        let log_callback = log_callback.clone();

        self.async_runtime.block_on(async move {
            self.try_send_log("TODO: Capture tracing spans from Rust ..")?;

            let log_thread = tokio::spawn(async move {
                loop {
                    match CString::new(format!("TODO: CAPTURE TRACE #003 ({:?})", log_callback)) {
                        Ok(c_string) => unsafe {
                            let log_callback = log_callback.lock().await;
                            log_callback(c_string.as_ptr());
                        }
                        Err(error) => {
                            tracing::error!("Log capture failed: {:}", error);
                        }
                    }
                    
                    // TODO: Remove this!
                    tokio::time::sleep(Duration::from_secs(5)).await;
                }
                
                0
            });
            
            // TODO: Remove this!
            tokio::time::sleep(Duration::from_nanos(100)).await;
            
            Ok(log_thread)
        })
    }

    /// TODO
    pub(crate) fn send_log<T: ToString>(&self, message: T) {
        // TODO: Re-enable this!
        // self.try_send_log(message).expect("Failed to send log message!");
    }
    
    pub(crate) fn try_send_log<T: ToString>(&self, message: T) -> Result<(), JsRuntimeError> {
        match CString::new(message.to_string()) {
            Ok(c_message) => match self.log_callback.as_ref() {
                Some(log_callback_mtx) => match log_callback_mtx.lock() {
                    Ok(log_callback) => unsafe {
                        log_callback(c_message.as_ptr());
                        Ok(())
                    }
                    Err(error) => Err(JsRuntimeError::from(error)),
                }
                
                None => Err(JsRuntimeError::LogCallbackMissing),
            }
            
            // Couldn't get CString, probably because (TODO).
            Err(error) => Err(JsRuntimeError::from(error)),
        }
    }

    /// TODO
    pub fn start(&self, main_specifier: &str) -> Result<u32, JsRuntimeError> {
        let stdio = self.stdio.try_clone_into()?;
        let current_dir = std::env::current_dir()?;

        // TODO: Move this to `ThetaRuntime::resolve_main_module(..)`.
        let main_module = resolve_url_or_path(main_specifier, &current_dir)?;

        // Run a "lite" Deno runtime, with only a core.
        //  - No worker and minimal extensions.
        //  - Useful for some testing and debug scenarios.
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

            if let Err(error) =
                js_runtime.execute_script_static("<prelude>", include_str!("./prelude.js"))
            {
                tracing::error!("Failed to run prelude script: {:}", error);
            }

            if let Err(error) =
                js_runtime.execute_script_static("<debug>", include_str!("./debug.js"))
            {
                tracing::error!("Failed to run debug setup script: {:}", error);
            }

            if let Err(error) = js_runtime
                .run_event_loop(PollEventLoopOptions::default())
                .await
            {
                tracing::error!("Failed to run main worker event loop: {:}", error);
            }
        });
        
        let inspector_addr = SocketAddr::V4(SocketAddrV4::new(Ipv4Addr::new(127, 0, 0, 1), 5622));
        let inspector_server = Arc::new(InspectorServer::new(inspector_addr, "asdf"));

        let mut worker = MainWorker::bootstrap_from_options(
            // TODO: Can we avoid cloning here?
            main_module.clone(),
            PermissionsContainer::allow_all(),
            WorkerOptions {
                stdio,
                bootstrap: self.create_bootstrap_options(),
                feature_checker: self.create_feature_checker(),
                module_loader: Rc::new(FsModuleLoader),
                origin_storage_dir: Some(std::path::PathBuf::from("./Data/Store")),
                maybe_inspector_server: Some(inspector_server),
                should_wait_for_inspector_session: false,
                extensions: vec![
                    //..
                ],
                ..Default::default()
            },
        );

        // Run the "not-lite", full Deno runtime.
        // Prefer this when you want all of Deno's features.
        #[cfg(not(feature = "lite"))]
        self.async_runtime.block_on(async move {
            // TODO: Revist the Clone for `main_module`.
            let error = worker.execute_main_module(&main_module.clone()).await?;
            
            // TODO
            worker.js_runtime.run_event_loop(PollEventLoopOptions::default()).await?;
            
            Ok(0)
        })
    }

    /// TODO
    fn create_bootstrap_options(&self) -> BootstrapOptions {
        BootstrapOptions {
            unstable_features: self.unstable_features.clone(),
            ..Default::default()
        }
    }

    /// TODO
    fn create_feature_checker(&self) -> Arc<FeatureChecker> {
        let mut feature_checker = FeatureChecker::default();

        for feature in UNSTABLE_GRANULAR_FLAGS.iter() {
            feature_checker.enable_feature(feature.0);
        }

        Arc::new(feature_checker)
    }
}

use deno_runtime::deno_core::anyhow::Error as DenoAnyError;

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
    NulError(std::ffi::NulError),

    /// TODO
    ModuleError(ModuleResolutionError),
    
    /// TODO
    DenoAnyError(DenoAnyError),
    
    /// An unknown error occurred.
    Unknown(&'static str),

    /// TODO
    LogCallbackMissing,

    /// TODO
    LogCallbackPoisoned,
}

impl Error for JsRuntimeError {}

impl Display for JsRuntimeError {
    /// TODO
    /// TODO
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_str("TODO")
    }
}

impl From<std::io::Error> for JsRuntimeError {
    /// TODO
    fn from(error: std::io::Error) -> JsRuntimeError {
        JsRuntimeError::ResourceError("io", error)
    }
}

impl From<std::ffi::NulError> for JsRuntimeError {
    /// TODO
    fn from(error: std::ffi::NulError) -> JsRuntimeError {
        JsRuntimeError::NulError(error)
    }
}

impl From<ModuleResolutionError> for JsRuntimeError {
    /// TODO
    fn from(error: ModuleResolutionError) -> JsRuntimeError {
        JsRuntimeError::ModuleError(error)
    }
}

impl From<DenoAnyError> for JsRuntimeError {
    /// TODO
    fn from(error: DenoAnyError) -> JsRuntimeError {
        JsRuntimeError::DenoAnyError(error)
    }
}

impl From<PoisonError<MutexGuard<'_, CLogCallback>>> for JsRuntimeError {
    /// TODO: Use the actual error!
    fn from(_: PoisonError<MutexGuard<'_, CLogCallback>>) -> JsRuntimeError {
        JsRuntimeError::LogCallbackPoisoned
    }
}

//---
#[cfg(feature="ffi")]
pub mod ffi {
    use std::ffi::CString;
    use std::path::Path;
    use std::sync::atomic::AtomicU32;
    use std::sync::atomic::Ordering;
    use std::sync::Arc;
    use std::sync::Mutex;
    use std::sync::OnceLock;
    use std::rc::Rc;
    use std::net::Ipv4Addr;
    use std::net::SocketAddr;
    use std::net::SocketAddrV4;

    use deno_runtime::deno_broadcast_channel::BroadcastChannel;
    use deno_runtime::deno_broadcast_channel::InMemoryBroadcastChannel;
    
    use tokio::runtime::Builder as TokioRuntimeBuilder;
    // use tokio::runtime::Runtime as TokioRuntime;

    use deno_runtime::worker::MainWorker;
    use deno_runtime::worker::WorkerOptions;
    use deno_runtime::permissions::PermissionsContainer;
    use deno_runtime::deno_core::FeatureChecker;
    use deno_runtime::deno_core::FsModuleLoader;
    // use deno_runtime::deno_core::PollEventLoopOptions;
    // use deno_runtime::deno_core::ModuleCodeString;
    // use deno_runtime::deno_net::DefaultTlsOptions;
    use deno_runtime::deno_core::resolve_url_or_path;
    use deno_runtime::deno_io::Stdio;
    use deno_runtime::deno_io::StdioPipe;
    use deno_runtime::inspector_server::InspectorServer;
    use deno_runtime::BootstrapOptions;
    use deno_runtime::UNSTABLE_GRANULAR_FLAGS;

    use crate::logging::ffi::CJsRuntimeLogLevel;
    use crate::logging::ffi::CLogCallback;
    use crate::start::ffi::CExecuteModuleOptions;
    use crate::start::ffi::CStartResult;

    use super::JsRuntimeConfig;
    use super::JsRuntimeManager;
    use super::JsRuntimeError;
        
    deno_runtime::deno_core::extension!(
        aby_sdk,
        // deps = [ deno_net ],
        // parameters = [
        //     P: NetPermissions
        // ],
        ops = [
            op_send_host_log,
            // ops::op_net_connect_tcp<P>,
        ],
        // esm_entry_point = "ext:aby_sdk/00_prelude.js",
        esm = [
            dir "src",
            // "00_entry.js",
        ],
        lazy_loaded_esm = [
            dir "src",
            "00_prelude.js",
            "99_debug.js",
        ],
        js = [
            // dir "src",
            // "00_aby.js"
        ],
        options = {
            some_bool_shit: Option<bool>,
            lol_strings: Option<Vec<String>>,
        },
        state = |state, options| {
            // state.put(SomeLogState {
            //     //..
            // });
        },
    );
    
    #[derive(Clone)]
    pub struct SomeLogState {
        //..
    }
    
    /// TODO
    #[deno_runtime::deno_core::op2(fast)]
    pub fn op_send_host_log(#[string] message: &str) {
        tracing::trace!("[Host]: {:}", message);
    }
    
    /// TODO
    #[deno_runtime::deno_core::op2(async)]
    #[serde] // TODO: Can we remove this?
    pub async fn op_send_host_log_async(
        // #[string] message: &str
    ) {
        tracing::trace!("[Host(Async)]: TODO");
    }
    
    #[derive(Debug)]
    #[repr(C)]
    pub struct CJsRuntime {
        config: CJsRuntimeConfig,
    }
    
    impl CJsRuntime {
        /// TODO
        #[allow(unused)] // TODO
        unsafe fn create_stdio<P: AsRef<Path>>(&self, dir: P) -> Result<Stdio, std::io::Error> {
            #[cfg(feature = "stdio")]
            {
                Ok(Stdio {
                    stdin: StdioPipe::File(deno_runtime::deno_io::STDIN_HANDLE.try_clone()?),
                    stdout: StdioPipe::File(deno_runtime::deno_io::STDOUT_HANDLE.try_clone()?),
                    stderr: StdioPipe::File(deno_runtime::deno_io::STDERR_HANDLE.try_clone()?),
                })
            }
            #[cfg(not(feature = "stdio"))]
            {
                let outpath = dir.as_ref().join("./AbyRuntime.out.log");
                let errpath = dir.as_ref().join("./AbyRuntime.err.log");
                
                Ok(Stdio {
                    stdin: StdioPipe::File(tempfile::tempfile()?), // TODO: Security audit lol.
                    stdout: StdioPipe::File({
                        std::fs::OpenOptions::new()
                            .read(true)
                            .write(true)
                            .create(true)
                            .open(outpath)?
                    }),
                    stderr: StdioPipe::File({
                        std::fs::OpenOptions::new()
                            .read(true)
                            .write(true)
                            .create(true)
                            .open(errpath)?
                    }),
                })
            }
        }
        
        /// TODO
        fn create_bootsrap_options(&self) -> BootstrapOptions {
            let unstable_features = {
                UNSTABLE_GRANULAR_FLAGS.iter()
                    .map(|&feature| feature.2)
                    .collect()
            };
            
            BootstrapOptions {
                unstable_features,
                ..Default::default()
            }
        }
        
        /// TODO
        fn create_feature_checker(&self) -> Arc<FeatureChecker> {
            let mut feature_checker = FeatureChecker::default();

            for feature in UNSTABLE_GRANULAR_FLAGS.iter() {
                feature_checker.enable_feature(feature.0);
            }

            Arc::new(feature_checker)
        }
    }
    
    #[export_name = "aby__construct_runtime"]
    pub unsafe extern "C" fn c_construct_runtime(config: CJsRuntimeConfig) -> *mut CJsRuntime {
        let js_runtime = CJsRuntime {
            config,
        };
        
        Box::into_raw(Box::new(js_runtime))
    }
    
    #[derive(Debug)]
    #[repr(C)]
    pub struct CExecModuleResult {
        code: CStartResult,
        message: Option<*const core::ffi::c_char>,
    }
    
    impl CJsRuntime {
        unsafe fn unwrap_ptr(ptr: &mut CJsRuntime) -> &mut CJsRuntime {
            // TODO: Ensure Pointer is safe to use.
            &mut *ptr
        }
        
        pub fn send_host_log<M: Into<String>>(&self, message: M) {
            if let Err(error) = self.try_send_host_log(message) {
                tracing::error!("Failed to send host message: {:}", error);
            }
        }
        
        pub fn try_send_host_log<M: Into<String>>(&self, message: M) -> Result<bool, std::io::Error> {
            match CString::new(message.into()) {
                Ok(message) => {
                    (self.config.log_callback_fn)(message.as_ptr());
                    Ok(true) // <3
                }
                Err(error) => Err(std::io::Error::other(format!("TODO: {:}", error)))
            }
        }
    }

    #[allow(unused, unreachable_code)]
    #[export_name = "aby__send_broadcast"]
    pub unsafe extern "C" fn c_send_broadcast(cself: *mut CJsRuntime, message: core::ffi::c_uint) {
        let js_runtime = CJsRuntime::unwrap_ptr(&mut *cself);
        
        // TODO: We need to keep this around. Options:
        //   - Store in a Boxed closure?
        //   - Pin Broadcast channel?
        let broadcast_channel = Box::new(InMemoryBroadcastChannel::default());
        
        let resource = todo!("Where do we get the resource?");
        let name = format!("Some broadcast channel ..");
        let data = vec![]; // TODO
        
        broadcast_channel.send(resource, name, data);
    }
    
    #[export_name = "aby__exec_module"]
    pub unsafe extern "C" fn c_exec_module(cself: *mut CJsRuntime, options: CExecuteModuleOptions) -> CStartResult {
        let cself = CJsRuntime::unwrap_ptr(&mut *cself);
            
        let Ok(async_runtime) = TokioRuntimeBuilder::new_current_thread().enable_time().enable_io().build() else {
            return CStartResult::FailedCreateAsyncRuntime;
        };

        let Ok(root_dir) = std::env::current_dir() else {
            return CStartResult::FailedFetchingWorkDirErr;
        };
        
        let Ok(data_dir) = crate::cwrap::try_unwrap_cstr(cself.config.db_dir) else {
            return CStartResult::DataDirInvalidErr;
        };
        
        let Ok(log_dir) = crate::cwrap::try_unwrap_cstr(cself.config.log_dir) else {
            return CStartResult::LogDirInvalidErr;
        };
        
        let Ok(main_module_specifier) = crate::cwrap::try_unwrap_cstr(options.main_module_specifier) else {
            return CStartResult::MainModuleInvalidErr;
        };
        
        // TODO: Move this to `ThetaRuntime::resolve_main_module(..)`.
        let Ok(main_module) = resolve_url_or_path(main_module_specifier, &root_dir) else {
            return CStartResult::MainModuleInvalidErr;
        };
        
        cself.send_host_log(format!("Resolved module to {:}", main_module));
        
        let Ok(stdio) = cself.create_stdio(&log_dir) else {
            return CStartResult::MainModuleUninitializedErr;
        };
        
        let maybe_inspector_server = {
            let inspector_name = "Aby Runtime 001";
            let inspector_addr = match SocketAddr::parse_ascii(b"asdf") {
                // let socket_addr = SocketAddrV4::new(Ipv4Addr::new(127, 0, 0, 1), cself.config.inspector_port);
                Ok(inspector_addr) => {
                    cself.send_host_log(format!("Using configured inspector address: {:}", inspector_addr));
                    inspector_addr
                }
                Err(error) => {
                    tracing::warn!("Failed to parse configured inspector address: {:}", error);
                    SocketAddr::V4(SocketAddrV4::new(Ipv4Addr::new(127, 0, 0, 1), 9222))
                }
            };
            
            cself.send_host_log(format!("Inspector address set to {:}", inspector_addr));
            
            Some(Arc::new(InspectorServer::new(inspector_addr, inspector_name)))
        };
        
        let should_wait_for_inspector_session = cself.config.inspector_wait && maybe_inspector_server.is_some();
        
        #[cfg(features = "verbose")]
        tracing::debug!("Executing Module: {:}", main_module);
        
        let mut worker = MainWorker::bootstrap_from_options(
            // TODO: Can we avoid cloning here?
            main_module.clone(),
            PermissionsContainer::allow_all(),
            WorkerOptions {
                stdio,
                bootstrap: cself.create_bootsrap_options(),
                feature_checker: cself.create_feature_checker(),
                module_loader: Rc::new(FsModuleLoader),
                origin_storage_dir: Some(std::path::PathBuf::from(data_dir)),
                extensions: vec![
                    aby_sdk::init_ops_and_esm(Some(true), None),
                ],
                maybe_inspector_server,
                should_wait_for_inspector_session,
                ..Default::default()
            },
        );
        
        // let aby_init_script = ModuleCodeString::Static(r#"
        //     import * as prelude from "ext:aby_sdk/src/00_prelude.js";
        // "#);

        // tracing::trace!("Aby Init Script:\n{:?}", aby_init_script);
        
        // TODO
        // if let Err(_) = worker.execute_script("<aby>", aby_init_script) {
        //     return CStartResult::Err;
        // }
        
        async_runtime.block_on(async move {
            // TODO: Revist the Clone for `main_module`.
            if let Err(error) = worker.execute_main_module(&main_module).await {
                cself.try_send_host_log(format!("Failed main module execution: {:}", error)).unwrap_or(false);
                return CStartResult::FailedModuleExecErr;
            }
            
            // TODO
            if let Err(error) = worker.run_event_loop(false).await {
                cself.try_send_host_log(format!("Failed to run main event loop: {:}", error)).unwrap_or(false);
                return CStartResult::FailedEventLoopErr;
            }
            
            CStartResult::Ok
        })
    }

    #[export_name = "aby__free_runtime"]
    pub unsafe extern "C" fn c_free_runtime(obj_ptr: *mut CJsRuntime) {
        let _ = Box::from_raw(obj_ptr);
    }
    
    //---
    /// TODO
    /// 
    /// Uses `OnceLock` for lazy init and lock, `Arc` for sharing,
    /// and `Mutex` for inner mutability.
    pub(crate) static JS_RUNTIME_MANAGER: OnceLock<Arc<Mutex<JsRuntimeManager>>> = OnceLock::new();

    /// TODO
    pub(crate) static JS_RUNTIME_STATE: AtomicU32 = AtomicU32::new(CJsRuntimeState::None as u32);

    /// TODO
    #[derive(Debug)]
    #[repr(C)]
    pub struct CJsRuntimeConfig {
        pub inspector_wait: bool,
        pub inspector_port: u16,
        pub db_dir: *const core::ffi::c_char,
        pub log_dir: *const core::ffi::c_char,
        pub log_level: CJsRuntimeLogLevel,
        pub log_callback_fn: CLogCallback,
    }
    
    impl TryInto<JsRuntimeConfig> for CJsRuntimeConfig {
        type Error = JsRuntimeError;
        fn try_into(self) -> Result<JsRuntimeConfig, Self::Error> {
            Ok(JsRuntimeConfig::new())
        }
    }
    
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
    #[export_name = "aby__get_state"]
    pub extern "C" fn c_get_state() -> CJsRuntimeState {
        match CJsRuntimeState::try_from(JS_RUNTIME_STATE.load(Ordering::Relaxed)) {
            Ok(state) => state,
            Err(error) => {
                tracing::error!("Couldn't get state: {:?}", error);
                CJsRuntimeState::None
            }
        }
    }
}