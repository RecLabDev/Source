use std::error::Error;
use std::fmt::Display;
use std::net::SocketAddr;
use std::path::Path;
use std::rc::Rc;
use std::sync::Arc;
use std::sync::Mutex;
use std::sync::MutexGuard;
use std::sync::PoisonError;

use tokio::runtime::Runtime as TokioRuntime;
use tokio::sync::Mutex as TokioMutex;

use deno_runtime::deno_broadcast_channel::BroadcastChannel;
use deno_runtime::deno_broadcast_channel::InMemoryBroadcastChannel;
use deno_runtime::deno_core::error::AnyError;
use deno_runtime::deno_core::resolve_url_or_path;
use deno_runtime::deno_core::url::Url;
use deno_runtime::deno_core::FeatureChecker;
use deno_runtime::deno_core::FsModuleLoader;
use deno_runtime::deno_core::ModuleResolutionError;
use deno_runtime::deno_io::Stdio;
use deno_runtime::deno_io::StdioPipe;
use deno_runtime::inspector_server::InspectorServer;
use deno_runtime::permissions::PermissionsContainer;
use deno_runtime::worker::MainWorker;
use deno_runtime::worker::WorkerOptions;
use deno_runtime::BootstrapOptions;
use deno_runtime::UNSTABLE_GRANULAR_FLAGS;

use crate::runtime::config::AbyRuntimeConfig;
use crate::runtime::state::AbyRuntimeState;
use crate::tracing::LoggingError;

#[cfg(feature = "ffi")]
use crate::tracing::ffi::CLogCallback;

//---
/// TODO
#[allow(unused)] // TODO: Remove this.
pub struct AbyRuntime {
    /// TODO
    config: AbyRuntimeConfig,

    /// TODO
    async_executor: Option<TokioRuntime>,

    /// TODO
    broadcast_channel: Option<InMemoryBroadcastChannel>,

    /// TODO: Remove this one.
    log_callback: Option<Arc<Mutex<CLogCallback>>>,

    /// TODO: Prefer this one.
    log_callback_async: Option<Arc<TokioMutex<CLogCallback>>>,
}

impl AbyRuntime {
    /// Constructs a new instance of `AbyRuntime` bootstrapped with only a
    /// configuration file and an async runtime.
    #[allow(unused_variables)] // TODO: Remove this.
    pub fn new(config: AbyRuntimeConfig) -> Self {
        AbyRuntime {
            config,
            async_executor: None,
            broadcast_channel: None,
            log_callback: None,
            log_callback_async: None,
        }
    }

    /// TODO
    pub fn with_async_runtime(mut self, async_runtime: TokioRuntime) -> Self {
        self.async_executor = Some(async_runtime);
        self // etc..
    }

    /// TODO
    pub fn with_broadcast_channel(mut self, broadcast_channel: InMemoryBroadcastChannel) -> Self {
        self.broadcast_channel = Some(broadcast_channel);
        self // etc..
    }

    /// TODO
    pub fn build(mut self) -> Self {
        self.broadcast_channel
            .get_or_insert_with(|| InMemoryBroadcastChannel::default());
        self // etc..
    }
}

impl AbyRuntime {
    /// TODO
    pub fn get_log_dir(&self) -> Result<&Path, AbyRuntimeConfigError> {
        self.config.log_dir()
            .ok_or_else(|| AbyRuntimeConfigError::MissingLogDir)
    }

    /// TODO
    pub fn get_root_dir(&self) -> Result<&Path, AbyRuntimeConfigError> {
        self.config.root_dir()
            .ok_or_else(|| AbyRuntimeConfigError::MissingRootDir)
    }

    /// TODO
    pub fn get_main_module_specifier(&self) -> Result<&str, AbyRuntimeConfigError> {
        self.config.main_module_specifier()
            .ok_or_else(|| AbyRuntimeConfigError::MissingMainModuleSpecifier)
    }

    /// TODO
    pub fn get_storage_dir(&self) -> Result<&Path, AbyRuntimeConfigError> {
        self.config.db_dir()
            .ok_or_else(|| AbyRuntimeConfigError::MissingInspectorAddr)
    }
    
    /// TODO
    fn _get_inspector_name(&self) -> Result<&str, AbyRuntimeConfigError> {
        self.config.inspector_name()
            .ok_or_else(|| AbyRuntimeConfigError::MissingInspectorName)
    }
    
    /// TODO
    fn get_async_executor(&self) -> Result<&TokioRuntime, AbyRuntimeError> {
        self.async_executor.as_ref()
            .ok_or_else(|| AbyRuntimeError::Uninitialized)
    }
    
    /// TODO
    fn get_inspector_addr(&self) -> Result<SocketAddr, AbyRuntimeConfigError> {
        SocketAddr::parse_ascii(self.config.inspector_addr().as_bytes())
            .map_err(|error| AbyRuntimeConfigError::InvalidInspectorAddr(error))
    }
    
    /// TODO
    fn get_broadcast_channel(&self) -> Result<InMemoryBroadcastChannel, AbyRuntimeError> {
        self.broadcast_channel.as_ref()
            .map(|broadcast| broadcast.clone())
            .ok_or(AbyRuntimeError::Uninitialized)
    }
    
    /// TODO
    #[cfg(not(feature = "stdio"))]
    fn create_stdio<P: AsRef<std::path::Path>>(&self, log_dir: P) -> Result<Stdio, std::io::Error> {
        use std::fs::OpenOptions;
        
        // TODO: Get log out name from config.
        let outpath = log_dir.as_ref().join("./AbyRuntime.out.log");
        let errpath = log_dir.as_ref().join("./AbyRuntime.err.log");
        
        Ok(Stdio {
            stdin: StdioPipe::File(tempfile::tempfile()?),
            stdout: StdioPipe::File(OpenOptions::new().read(true).write(true).create(true).open(outpath)?),
            stderr: StdioPipe::File(OpenOptions::new().read(true).write(true).create(true).read(true).write(true).create(true).open(errpath)?),
        })
    }
    
    /// User has enabled the `stdio` feature, so we just grab Deno's
    /// preferred handles for stdio.
    #[cfg(feature = "stdio")]
    fn create_stdio(&self) -> Result<Stdio, std::io::Error> {
        let stdin = deno_runtime::deno_io::STDIN_HANDLE.try_clone()?;
        let stdout = deno_runtime::deno_io::STDOUT_HANDLE.try_clone()?;
        let stderr = deno_runtime::deno_io::STDERR_HANDLE.try_clone()?;

        Ok(Stdio {
            stdin: StdioPipe::File(stdin),
            stdout: StdioPipe::File(stdout),
            stderr: StdioPipe::File(stderr),
        })
    }
    
    /// TODO: Move this to regular AbyRuntime.
    fn create_bootsrap_options(&self) -> BootstrapOptions {
        BootstrapOptions {
            unstable_features: Vec::from(self.config.unstable_deno_features()),
            ..Default::default()
        }
    }
    
    /// TODO: Move this to regular `AbyRuntime``.
    fn create_feature_checker(&self) -> Arc<FeatureChecker> {
        let mut feature_checker = FeatureChecker::default();

        for feature in UNSTABLE_GRANULAR_FLAGS.iter() {
            feature_checker.enable_feature(feature.0);
        }

        #[cfg(feature = "verbose")]
        tracing::debug!("Creating feature checker:\n{:#?}", feature_checker);

        Arc::new(feature_checker)
    }
    
    /// TODO
    fn create_inspector_server(&self) -> Result<InspectorServer, AbyRuntimeError> {
        // TODO: let inspector_name = format!("{:}", self.get_inspector_name()?);
        Ok(InspectorServer::new(
            self.get_inspector_addr()?,
            "Aby Runtime Inspector",
        ))
    }
    
    /// TODO
    fn create_worker(&self) -> Result<MainWorker, AbyRuntimeError> {
        // TODO: Get these from config input ..
        let main_module_url = self.resolve_main_module_url()?;
        
        #[cfg(feature = "stdio")]
        let stdio = self.create_stdio()?;
        
        #[cfg(not(feature = "stdio"))]
        let stdio = self.create_stdio(self.get_log_dir()?)?;
        
        let bootstrap = self.create_bootsrap_options();
        
        let permissions_container = PermissionsContainer::allow_all();
        let feature_checker = self.create_feature_checker();
        
        let origin_storage_dir = self.get_storage_dir()?;
        let maybe_inspector_server = self.create_inspector_server()?;
        let broadcast_channel = self.get_broadcast_channel()?;
        
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
            main_module_url,
            permissions_container,
            WorkerOptions {
                stdio,
                bootstrap,
                feature_checker,
                skip_op_registration: false,
                broadcast_channel,
                module_loader: Rc::new(FsModuleLoader),
                origin_storage_dir: Some(origin_storage_dir.to_owned()),
                maybe_inspector_server: Some(Arc::new(maybe_inspector_server)),
                should_wait_for_inspector_session: self.config.inspector_wait(),
                extensions: vec![aby_sdk::init_ops_and_esm(Some(true), None)],
                ..Default::default()
            },
        ))
    }
    
    /// TODO
    pub fn resolve_main_module_url(&self) -> Result<Url, AbyRuntimeError> {
        self.resolve_module_url(self.get_main_module_specifier()?)
    }
    
    /// TODO
    fn resolve_module_url(&self, module_specifier: &str) -> Result<Url, AbyRuntimeError> {
        resolve_url_or_path(module_specifier, &self.get_root_dir()?)
            .map_err(|error| AbyRuntimeError::InvalidModuleSpecifier(error))
    }
    
    /// TODO
    pub async fn send_broadcast(&self) -> Result<bool, AbyRuntimeError> {
        let broadcast = self.get_broadcast_channel()?;
        let resource = broadcast.subscribe()?;

        let name = format!("ABY_BIFROST");
        let data = vec![]; // TODO

        if let Err(error) = broadcast.send(&resource, name, data).await {
            return Err(AbyRuntimeError::FailedBroadcastSend(error));
        }

        Ok(true)
    }
    
    /// TODO
    pub fn send_broadcast_sync(&self) -> Result<bool, AbyRuntimeError> {
        self.get_async_executor()?.block_on(async {
            self.send_broadcast().await
        })
    }
    
    /// TODO
    pub fn exec_sync(&self, exec_module_specifier: &str) -> Result<bool, AbyRuntimeError> {
        self.get_async_executor()?.block_on(async {
            self.exec(exec_module_specifier).await
        })
    }
    
    /// TODO
    pub async fn exec(&self, exec_module_specifier: &str) -> Result<bool, AbyRuntimeError> {
        #[cfg(features = "dev")]
        tracing::debug!("Executing Module: {:}", exec_module_specifier);
        
        let exec_module_url = self.resolve_module_url(exec_module_specifier)?;
        let mut worker = self.create_worker()?;
        
        worker.execute_main_module(&exec_module_url).await?;
        worker.run_event_loop(false).await?;
        
        // TODO: Collect (and return) run report ..
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
}

//---
/// TODO
#[derive(oops::Error)]
pub enum AbyRuntimeError {
    /// AbyRuntime wasn't initialized yet (and/or properly).
    #[msg("aby runtime not yet initialized")]
    Uninitialized,

    /// TODO
    #[msg("resource '{0:}' error; {1:}")]
    ResourceError(&'static str, std::io::Error),

    /// TODO
    #[msg("logging setup failed: {0:}")]
    LoggingSetupFailed(LoggingError),

    /// TODO
    #[msg("invalid runtime config: {0:}")]
    InvalidConfig(AbyRuntimeConfigError),

    /// The runtime detected a current or future invalid atomic state.
    #[msg("invalid state '{0:}'")]
    InvalidState(u32),

    /// TODO
    #[msg("invalid module specifier '{0:}'")]
    InvalidModuleSpecifier(ModuleResolutionError),

    /// TODO
    #[msg("failed module resolution: {0:}")]
    FailedModuleResolution(ModuleResolutionError),

    /// TODO
    #[msg("failed to send broadcast: {0:}")]
    FailedBroadcastSend(AnyError),

    /// TODO
    #[msg("unbeknownst error: {0:}")]
    AnyError(AnyError),

    // /// An unknown error occurred.
    // #[msg("unknown error: {0:}")]
    // Unknown(&'static str),

    //---
    /// TODO: Move this one to CAbyRuntimeError.
    #[msg("marshal failed: {0:}")]
    NulError(std::ffi::NulError),

    /// TODO: Move this one to CAbyRuntimeError.
    #[msg("invalid main module '{0:}'")]
    InvalidCBinding(cwrap::error::CStringError),
}

#[derive(oops::Error)]
pub enum AbyRuntimeConfigError {
    /// Indicates the root dir couldn't be resolved from any registered source.
    ///
    /// In the case of a runtime which provides a sensible default (like the
    /// current working directory on Windows/macOS/*nix/etc.), this usually
    /// means we both didn't find a project root specified in configs or args,
    /// but we also failed to look up a sensible default.
    #[msg("missing root dir")]
    MissingRootDir,

    /// TODO
    #[msg("missing main module specifier")]
    MissingMainModuleSpecifier,

    /// TODO
    #[msg("missing log dir")]
    MissingLogDir,

    /// TODO
    #[msg("missing data dir")]
    MissingDataDir,

    /// TODO
    #[msg("missing inspector name")]
    MissingInspectorName,

    /// TODO
    #[msg("missing inspector addr")]
    MissingInspectorAddr,

    /// TODO
    #[msg("invalid inspector addr '{0:}'")]
    InvalidInspectorAddr(std::net::AddrParseError),
}

impl From<cwrap::error::CStringError> for AbyRuntimeError {
    /// TODO
    fn from(error: cwrap::error::CStringError) -> AbyRuntimeError {
        AbyRuntimeError::InvalidCBinding(error)
    }
}

impl From<ModuleResolutionError> for AbyRuntimeError {
    /// TODO
    fn from(error: ModuleResolutionError) -> AbyRuntimeError {
        AbyRuntimeError::FailedModuleResolution(error)
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

impl From<AbyRuntimeConfigError> for AbyRuntimeError {
    /// TODO
    fn from(error: AbyRuntimeConfigError) -> AbyRuntimeError {
        AbyRuntimeError::InvalidConfig(error)
    }
}

impl From<PoisonError<MutexGuard<'_, CLogCallback>>> for AbyRuntimeError {
    /// TODO: Use the actual error!
    fn from(_: PoisonError<MutexGuard<'_, CLogCallback>>) -> AbyRuntimeError {
        AbyRuntimeError::LoggingSetupFailed(LoggingError::LogCallbackPoisoned)
    }
}
