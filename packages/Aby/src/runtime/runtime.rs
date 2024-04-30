use std::rc::Rc;
use std::sync::Arc;
use std::sync::Mutex;
use std::sync::MutexGuard;
use std::sync::PoisonError;
use std::path::PathBuf;
#[cfg(not(feature = "stdio"))]
use std::fs::OpenOptions;
use std::net::Ipv4Addr;
use std::net::SocketAddr;
use std::net::SocketAddrV4;
use std::error::Error;
use std::fmt::Display;

use tokio::runtime::Runtime as TokioRuntime;
use tokio::sync::Mutex as TokioMutex;
// use tokio::sync::broadcast;

use deno_runtime::BootstrapOptions;
use deno_runtime::UNSTABLE_GRANULAR_FLAGS;
use deno_runtime::worker::MainWorker;
use deno_runtime::worker::WorkerOptions;
use deno_runtime::permissions::PermissionsContainer;
use deno_runtime::deno_core::error::AnyError;
use deno_runtime::deno_core::url::Url;
use deno_runtime::deno_core::ModuleResolutionError;
use deno_runtime::deno_core::FeatureChecker;
use deno_runtime::deno_core::FsModuleLoader;
use deno_runtime::deno_core::resolve_url_or_path;
use deno_runtime::deno_io::Stdio;
use deno_runtime::deno_io::StdioPipe;
use deno_runtime::inspector_server::InspectorServer;
use deno_runtime::deno_broadcast_channel::BroadcastChannel;
use deno_runtime::deno_broadcast_channel::InMemoryBroadcastChannel;
// use deno_runtime::deno_broadcast_channel::InMemoryBroadcastChannelResource;
// use deno_runtime::deno_broadcast_channel::deno_broadcast_channel;

use crate::tracing::LoggingError;

#[cfg(feature = "ffi")]
use crate::tracing::ffi::CLogCallback;

/// TODO
pub const DEFAULT_INSPECTOR_SOCKET_ADDR: SocketAddrV4 = SocketAddrV4::new(Ipv4Addr::new(127, 0, 0, 1), 9222);

// /// TODO: Probably remove this.
// /// 
// /// Uses `OnceLock` for lazy init and lock, `Arc` for sharing,
// /// and `Mutex` for inner mutability.
// pub(crate) static JS_RUNTIME_MANAGER: OnceLock<Arc<Mutex<AbyRuntime>>> = OnceLock::new();

//---
/// TODO
#[allow(unused)] // TODO: Remove this.
pub struct AbyRuntime {
    /// TODO
    config: AbyRuntimeConfig,
    
    /// TODO
    async_runtime: Option<TokioRuntime>,
    
    /// TODO
    broadcast: Option<InMemoryBroadcastChannel>,

    /// TODO: Remove this one.
    log_callback: Option<Arc<Mutex<CLogCallback>>>,
    
    /// TODO: Prefer this one.
    log_callback_async: Option<Arc<TokioMutex<CLogCallback>>>,
}

impl AbyRuntime {
    /// TODO
    pub const DEFAULT_INSPECTOR_ADDR: &'static str = "localhost:9222";
    
    /// TODO
    pub const DEFAULT_MAIN_MODULE_SPECIFIER: &'static str = "./src/main.js";
    
    /// Constructs a new instance of `AbyRuntime` bootstrapped with only a 
    /// configuration file and an async runtime.
    #[allow(unused_variables)] // TODO: Remove this.
    pub fn new(config: AbyRuntimeConfig) -> Self {
        AbyRuntime {
            config,
            async_runtime: None,
            broadcast: None,
            log_callback: None,
            log_callback_async: None,
        }
    }
    
    pub fn with_async_runtime(mut self, async_runtime: TokioRuntime) -> Self {
        self.async_runtime = Some(async_runtime);
        self // etc..
    }
    
    pub fn with_broadcast_channel(mut self, broadcast_channel: InMemoryBroadcastChannel) -> Self {
        self.broadcast = Some(broadcast_channel);
        self // etc..
    }
    
    /// TODO
    pub fn build(mut self) -> Self {
        if self.broadcast.is_none() {
            self.broadcast = Some(InMemoryBroadcastChannel::default());
        }
        
        if self.async_runtime.is_none() {
            // TOOD: Fail here.
        }
        
        self
    }
    
    /// TODO
    fn get_data_dir(&self) -> Result<PathBuf, AbyRuntimeError> {
        match &self.config.db_dir {
            Some(data_dir) => Ok(PathBuf::from(data_dir)),
            None => Err(AbyRuntimeError::Unknown("TODO: failed to get data dir"))
        }
    }
    
    /// TODO
    #[cfg(not(feature = "stdio"))]
    fn get_log_dir(&self) -> Result<PathBuf, AbyRuntimeError> {
        match &self.config.log_dir {
            Some(log_dir) => Ok(PathBuf::from(log_dir)),
            None => Err(AbyRuntimeError::Unknown("TODO: failed to get log dir"))
        }
    }
    
    /// TODO
    #[allow(unused_variables)] // TODO: Remove this.
    pub fn get_root_dir(&self) -> Result<PathBuf, AbyRuntimeError> {
        match self.config.root_dir.as_ref() {
            Some(root_dir) => Ok(PathBuf::from(root_dir)),
            None => Ok(std::env::current_dir()?),
        }
    }
    
    /// TODO
    /// 
    /// TODO: We only want the default value when we can't ;
    pub fn get_main_module_url(&self) -> Result<Url, AbyRuntimeError> {
        match self.config.main_module_specifier {
            Some(ref main_module_specifier) => self.resolve_module_specifier(main_module_specifier),
            None => Err(AbyRuntimeError::Uninitialized),
        }
    }
    
    /// TODO: Move this to the config.
    pub fn inspector_addr(&self) -> String {
        self.config.inspector_addr.to_owned().unwrap_or_else(|| String::from("localhost:9222"))
    }
    
    /// TODO
    pub fn default_main_module_specifier(&self) -> Url {
        Url::from_file_path(Self::DEFAULT_MAIN_MODULE_SPECIFIER).expect("main module specifier")
    }
    
    /// User has enabled the `stdio` feature, so we just grab Deno's 
    /// preferred handles for stdio.
    #[cfg(feature = "stdio")]
    fn get_stdio(&self) -> Result<Stdio, std::io::Error> {
        Ok(Stdio {
            stdin: StdioPipe::File(deno_runtime::deno_io::STDIN_HANDLE.try_clone()?),
            stdout: StdioPipe::File(deno_runtime::deno_io::STDOUT_HANDLE.try_clone()?),
            stderr: StdioPipe::File(deno_runtime::deno_io::STDERR_HANDLE.try_clone()?),
        })
    }
    
    /// Cont. >> Otherwise, we'll just pipe to log files for now.
    /// TODO: Evaluate safer, more managed stdio aggregate methods.
    #[cfg(not(feature = "stdio"))]
    fn create_stdio<P: AsRef<Path>>(&self, log_dir: P) -> Result<Stdio, std::io::Error> {
        let outpath = log_dir.as_ref().join("./AbyRuntime.out.log");
        let errpath = log_dir.as_ref().join("./AbyRuntime.err.log");
        
        Ok(Stdio {
            stdin: StdioPipe::File(tempfile::tempfile()?),
            stdout: StdioPipe::File(std::fs::OpenOptions::new().read(true).write(true).create(true).open(outpath)?),
            stderr: StdioPipe::File(std::fs::OpenOptions::new().read(true).write(true).create(true).open(errpath)?),
        })
    }
    
    /// TODO: Move this to regular AbyRuntime.
    fn get_bootsrap_options(&self) -> BootstrapOptions {
        let unstable_features = self.config.unstable_deno_features.to_owned();
        
        BootstrapOptions {
            unstable_features,
            ..Default::default()
        }
    }
    
    /// TODO: Move this to regular `AbyRuntime``.
    fn get_feature_checker(&self) -> Arc<FeatureChecker> {
        let mut feature_checker = FeatureChecker::default();

        for feature in UNSTABLE_GRANULAR_FLAGS.iter() {
            feature_checker.enable_feature(feature.0);
        }

        #[cfg(feature = "verbose")]
        tracing::debug!("Unstable features: {:?}", unstable_features);
        
        Arc::new(feature_checker)
    }
    
    /// TODO
    pub fn create_inspector_server(&self) -> Result<InspectorServer, AbyRuntimeError> {
        let inspector_name: &str = "Aby Runtime 001";
        let inspector_addr: String = self.inspector_addr();
        
        let inspector_addr = match SocketAddr::parse_ascii(inspector_addr.as_bytes()) {
            Ok(inspector_addr) => inspector_addr,
            Err(error) => {
                tracing::warn!("Failed to parse inspector addr '{:}': {:}", inspector_addr, error);
                #[cfg(feature = "verbose")]
                {
                    // TODO: We should probably bail here, since starting with 
                    //  a default when we've given explicit instructions could
                    //  lead to unexpected and difficult to debug behavior.
                    tracing::debug!("Falling back to default inspectr addr '{:?}'", DEFAULT_INSPECTOR_SOCKET_ADDR);
                }
                
                SocketAddr::V4(DEFAULT_INSPECTOR_SOCKET_ADDR)
            }
        };
        
        Ok(InspectorServer::new(inspector_addr, inspector_name))
    }
    
    /// TODO
    fn create_broadcast_channel(&self) -> Result<InMemoryBroadcastChannel, AbyRuntimeError> {
        Ok(InMemoryBroadcastChannel::default())
    }
    
    /// TODO
    #[allow(unused_variables)] // TODO: Remove this.
    fn resolve_module_specifier(&self, module_specifier: &str) -> Result<Url, AbyRuntimeError> {
        resolve_url_or_path(module_specifier, &self.get_root_dir()?)
            .map_err(|error| {
                // TODO: Expose the actual error!
                AbyRuntimeError::Unknown("TODO: failed to resolve module at specifier")
            })
    }
    
    /// TODO
    pub fn create_worker(&self) -> Result<MainWorker, AbyRuntimeError> {
        // TODO: Get these from config input ..
        let main_module_specifier = self.get_main_module_url()?;
        
        #[cfg(feature = "stdio")]
        let stdio = self.get_stdio()?;
        
        #[cfg(not(feature = "stdio"))]
        let stdio = self.create_stdio(self.get_log_dir()?)?;
        
        let bootstrap = self.get_bootsrap_options();
        
        let permissions_container = PermissionsContainer::allow_all();
        let feature_checker = self.get_feature_checker();
        
        let origin_storage_dir = self.get_data_dir()?;
        let maybe_inspector_server = self.create_inspector_server()?;
        let broadcast_channel = self.create_broadcast_channel()?;
        
        // let aby_init_script = ModuleCodeString::Static(r#"
        //     import * as prelude from "ext:aby_sdk/src/00_prelude.js";
        // "#);

        // tracing::trace!("Aby Init Script:\n{:?}", aby_init_script);
        
        // /// TODO
        // if let Err(_) = worker.execute_script("<aby>", aby_init_script) {
        //     return CExecModuleResult::Err;
        // }
        
        // TODO
        deno_runtime::deno_core::extension!(
            aby_sdk,
            // deps = [ deno_net ],
            // parameters = [
            //     P: NetPermissions
            // ],
            ops = [
                crate::tracing::ops::op_send_host_log,
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
                state.put(AbyRuntimeState {
                    //..
                });
            },
        );
        
        Ok(MainWorker::bootstrap_from_options(
            main_module_specifier,
            permissions_container,
            WorkerOptions {
                stdio,
                bootstrap,
                feature_checker,
                skip_op_registration: false,
                broadcast_channel,
                module_loader: Rc::new(FsModuleLoader),
                origin_storage_dir: Some(origin_storage_dir),
                maybe_inspector_server: Some(Arc::new(maybe_inspector_server)),
                should_wait_for_inspector_session: self.config.inspector_wait,
                extensions: vec![
                    aby_sdk::init_ops_and_esm(Some(true), None),
                ],
                ..Default::default()
            }
        ))
    }
    
    /// TODO
    pub fn send_broadcast(&self) -> Result<bool, AbyRuntimeError> {
        let broadcast = self.broadcast.as_ref().ok_or(AbyRuntimeError::Uninitialized)?;
        
        let Ok(resource) = broadcast.subscribe() else {
            return Err(AbyRuntimeError::Unknown("TODO: failed to subscribe to broadcast channel"))
        };
        
        let name = format!("Some broadcast channel ..");
        let data = vec![]; // TODO
        
        let _error = broadcast.send(&resource, name, data);
        
        Ok(true)
    }
    
    /// TODO
    pub fn collect_error(error: impl Error + Display) {
        // TODO: If we get to this point it means we're trying to use our
        //  fallback and even that isn't working. We don't typically want to
        //  kill the whole runtime at this point since something else up
        //  the stream might want to try to recover. In this case, we should
        //  set a "mount failed" switch/alert somewhere.
        tracing::error!("{:}", error)
    }
    
    /// TODO
    #[allow(unused_variables)] // TODO: Remove this.
    pub fn exec_sync(&self, exec_module_specifier: &str) -> Result<bool, AbyRuntimeError> {
        let exec_module_specifier = self.resolve_module_specifier(exec_module_specifier)?;
        
        #[cfg(features = "dev")]
        tracing::debug!("Executing Module: {:}", exec_module_specifier);
        
        let mut worker = self.create_worker()?;
        let async_runtime = self.async_runtime.as_ref().ok_or(AbyRuntimeError::Uninitialized)?;
        
        async_runtime.block_on(async move {
            worker.execute_main_module(&exec_module_specifier).await?;
            worker.run_event_loop(false).await?;
            Ok(true)
        })
    }
}

//---
/// TODO
#[derive(Default, Debug)]
pub struct AbyRuntimeConfig {
    /// TODO
    main_module_specifier: Option<String>,
    
    /// TODO
    root_dir: Option<PathBuf>,
    
    /// TODO
    db_dir: Option<PathBuf>,
    
    /// TODO
    log_dir: Option<PathBuf>,
    
    /// TODO
    inspector_addr: Option<String>,
    
    /// TODO
    inspector_wait: bool,

    /// Add support for as-yet unstable Deno features within the current
    /// instance of `AbyRuntime`. See deno docs (below) for more info.
    /// 
    /// ### Resources:
    /// -   [Unstable Flags Documentation](https://docs.deno.com/runtime/manual/tools/unstable_flags)
    /// -   [Source Mapping for key->i32](https://github.com/denoland/deno/blob/1fadb940f41f4f9f78e616c90008a31a44dc28bc/runtime/lib.rs#L49)
    /// 
    /// ### Known Features
    /// 1.  BroadcastChannel
    /// 2.  Deno.cron
    /// 3.  FFI
    /// 4.  File System
    /// 5.  HTTP
    /// 6.  Key-Value
    /// 7.  Net
    /// 8.  Temporal
    /// 9.  Proto
    /// 10. WebGPU
    /// 11. Web Worker
    unstable_deno_features: Vec<i32>,
}

impl AbyRuntimeConfig {
    /// TODO
    pub fn new() -> Self {
        AbyRuntimeConfig::default()
    }
    
    /// TODO
    pub fn with_main_module_specifier<V: Into<String>>(mut self, value: V) -> Self {
        self.main_module_specifier = Some(value.into());
        self // etc..
    }
    
    /// TODO
    pub fn with_root_dir<V: Into<String>>(mut self, value: V) -> Self {
        self.root_dir = Some(PathBuf::from(value.into()));
        self // etc..
    }
    
    /// TODO
    pub fn with_db_dir<V: Into<String>>(mut self, value: V) -> Self {
        self.db_dir = Some(PathBuf::from(value.into()));
        self // etc..
    }
    
    /// TODO
    pub fn with_log_dir<V: Into<String>>(mut self, value: V) -> Self {
        self.log_dir = Some(PathBuf::from(value.into()));
        self // etc..
    }
    
    /// TODO
    pub fn with_inspector_addr<V: Into<String>>(mut self, value: V) -> Self {
        self.inspector_addr = Some(value.into());
        self // etc..
    }
    
    /// TODO
    pub fn with_inspector_wait<V: Into<bool>>(mut self, value: V) -> Self {
        self.inspector_wait = value.into();
        self // etc..
    }
    
    /// TODO
    pub fn with_unstable_deno_features<V: Into<Vec<i32>>>(mut self, value: V) -> Self {
        self.unstable_deno_features = value.into();
        self // etc..
    }
}

/// TODO
#[derive(Clone)]
pub struct AbyRuntimeState {
    //..
}

//---
/// TODO
#[derive(oops::Error)]
pub enum AbyRuntimeError {
    /// AbyRuntime wasn't initialized yet (and/or properly).
    #[msg("aby runtime not yet initialized")]
    Uninitialized,
    
    /// A user-supplied module-name was invalid.
    #[msg("invalid module specifier '{0:}'")]
    InvalidModuleSpecifier(&'static str),
    
    /// The runtime detected a current or future invalid atomic state.
    #[msg("invalid state id '{0:}'")]
    InvalidState(u32),
    
    /// TODO
    #[msg("resource '{0:}' error; {1:}")]
    ResourceError(&'static str, std::io::Error),
    
    /// TODO
    #[msg("logging setup failed: {0:}")]
    LoggingSetupFailed(LoggingError),
    
    /// TODO: Move this one to CAbyRuntimeError.
    #[msg("marshal failed: {0:}")]
    NulError(std::ffi::NulError),
    
    /// TODO: Move this one to CAbyRuntimeError.
    #[msg("invalid main module '{0:}'")]
    InvalidMainModule(cwrap::error::CStringError),
    
    /// TODO
    #[msg("failed module resolution: {0:}")]
    FailedResolution(ModuleResolutionError),
    
    /// TODO
    #[msg("unbeknownst error: {0:}")]
    AnyError(AnyError),
    
    /// An unknown error occurred.
    #[msg("unknown error: {0:}")]
    Unknown(&'static str),
}

impl From<cwrap::error::CStringError> for AbyRuntimeError {
    /// TODO
    fn from(error: cwrap::error::CStringError) -> AbyRuntimeError {
        AbyRuntimeError::InvalidMainModule(error)
    }
}

impl From<ModuleResolutionError> for AbyRuntimeError {
    /// TODO
    fn from(error: ModuleResolutionError) -> AbyRuntimeError {
        AbyRuntimeError::FailedResolution(error)
    }
}

impl From<std::io::Error> for AbyRuntimeError {
    /// TODO
    fn from(error: std::io::Error) -> AbyRuntimeError {
        AbyRuntimeError::ResourceError("io", error)
    }
}

impl From<std::ffi::NulError> for AbyRuntimeError {
    /// TODO
    fn from(error: std::ffi::NulError) -> AbyRuntimeError {
        AbyRuntimeError::NulError(error)
    }
}

impl From<AnyError> for AbyRuntimeError {
    /// TODO
    fn from(error: AnyError) -> AbyRuntimeError {
        AbyRuntimeError::AnyError(error)
    }
}

impl From<PoisonError<MutexGuard<'_, CLogCallback>>> for AbyRuntimeError {
    /// TODO: Use the actual error!
    fn from(_: PoisonError<MutexGuard<'_, CLogCallback>>) -> AbyRuntimeError {
        AbyRuntimeError::LoggingSetupFailed(LoggingError::LogCallbackPoisoned)
    }
}
