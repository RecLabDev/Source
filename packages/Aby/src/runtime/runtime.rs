use std::net::SocketAddr;
use std::path::Path;
use std::rc::Rc;
use std::sync::Arc;

use tokio::runtime::Builder as TokioRuntimeBuilder;
use tokio::runtime::Runtime as TokioRuntime;

use deno_runtime::BootstrapOptions;
use deno_runtime::UNSTABLE_GRANULAR_FLAGS;
use deno_runtime::worker::MainWorker;
use deno_runtime::worker::WorkerOptions;
use deno_runtime::permissions::PermissionsContainer;
use deno_runtime::inspector_server::InspectorServer;
use deno_runtime::deno_core::error::AnyError;
use deno_runtime::deno_core::resolve_url_or_path;
use deno_runtime::deno_core::url::Url;
use deno_runtime::deno_core::FeatureChecker;
use deno_runtime::deno_core::FsModuleLoader;
use deno_runtime::deno_core::ModuleResolutionError;
use deno_runtime::deno_io::Stdio;
use deno_runtime::deno_io::StdioPipe;
use deno_runtime::deno_broadcast_channel::BroadcastChannel;
use deno_runtime::deno_broadcast_channel::InMemoryBroadcastChannel;

use crate::tracing::LoggingError;
use crate::runtime::config::AbyRuntimeConfig;
use crate::runtime::config::AbyRuntimeConfigError;
use crate::runtime::state::AbyRuntimeState;

//---
/// TODO
// #[allow(unused)] // TODO: Remove this.
pub struct AbyRuntime {
    /// TODO
    config: AbyRuntimeConfig,
    
    /// TODO
    async_executor: TokioRuntime,
    
    /// TODO
    broadcast_channel: InMemoryBroadcastChannel,
    
    /// TODO
    feature_checker: Arc<FeatureChecker>,
}

impl AbyRuntime {
    /// Constructs a new instance of `AbyRuntime` bootstrapped with only a
    /// configuration file and an async runtime.
    pub fn new(config: AbyRuntimeConfig) -> Result<Self, AbyRuntimeError> {
        // TODO: Create the async runtime in`aby_runtime.build()`.
        let async_executor = {
            TokioRuntimeBuilder::new_current_thread()
                .enable_all()
                .build()?
        };

        #[cfg(feature = "verbose")]
        tracing::debug!("Creating feature checker:\n{:#?}", feature_checker);
        
        let mut feature_checker = FeatureChecker::default();
        for feature in UNSTABLE_GRANULAR_FLAGS.iter() {
            feature_checker.enable_feature(feature.0);
        }
        
        let runtime = AbyRuntime {
            config,
            async_executor,
            broadcast_channel: InMemoryBroadcastChannel::default(),
            feature_checker: Arc::new(feature_checker),
        };

        // TODO: Print state of whole runtime ..
        #[cfg(feature = "verbose")]
        tracing::debug!("Created feature checker:\n{:#?}", runtime.feature_checker);

        Ok(runtime)
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
    pub fn get_inspector_name(&self) -> Result<&str, AbyRuntimeConfigError> {
        self.config.inspector_name()
            .ok_or_else(|| AbyRuntimeConfigError::MissingInspectorName)
    }
    
    /// TODO
    pub fn get_inspector_addr(&self) -> Result<SocketAddr, AbyRuntimeConfigError> {
        SocketAddr::parse_ascii(self.config.inspector_addr().as_bytes())
            .map_err(|error| AbyRuntimeConfigError::InvalidInspectorAddr(error))
    }
    
    /// TODO
    pub fn get_broadcast_channel(&self) -> InMemoryBroadcastChannel {
        self.broadcast_channel.clone()
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
    
    /// TODO
    fn create_bootsrap_options(&self) -> BootstrapOptions {
        BootstrapOptions {
            unstable_features: Vec::from(self.config.unstable_deno_features()),
            ..Default::default()
        }
    }
    
    /// TODO
    fn create_inspector_server(&self) -> Result<InspectorServer, AbyRuntimeError> {
        Ok(InspectorServer::new(self.get_inspector_addr()?, "Aby Runtime Inspector"))
    }
    
    /// TODO
    fn create_worker(&self) -> Result<MainWorker, AbyRuntimeError> {
        use std::time::Instant;
        
        let start = Instant::now();
        
        // TODO: Get these from config input ..
        let main_module_url = self.resolve_main_module_url()?;
        
        // In most Rust-only scenarios we can just use the usual stdio.
        #[cfg(feature = "stdio")]
        let stdio = self.create_stdio()?;
        
        // In ffi and other extern scenarios, we often want to re-route stdio
        // to some other logging facility (file, editor display, etc).
        #[cfg(not(feature = "stdio"))]
        let stdio = self.create_stdio(self.get_log_dir()?)?;
        
        let bootstrap = self.create_bootsrap_options();
        
        // TODO: Hold the permissions container in Self ..
        let permissions_container = PermissionsContainer::allow_all();
        let feature_checker = self.feature_checker.clone();
        
        let origin_storage_dir = self.get_storage_dir()?;
        
        let maybe_inspector_server = self.create_inspector_server()?;
        let broadcast_channel = self.broadcast_channel.clone();
        
        // TODO: Evaluate whether or not we can do this in a JIT-less context.
        // let aby_init_script = ModuleCodeString::Static(r#"
        //     import * as prelude from "ext:aby_sdk/src/00_prelude.js";
        // "#);
        // 
        // #[cfg(feature = "debug")]
        // tracing::debug!("Aby Init Script:\n{:?}", aby_init_script);
        
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
        
        let worker = MainWorker::bootstrap_from_options(
            main_module_url,
            permissions_container,
            WorkerOptions {
                stdio,
                bootstrap,
                // startup_snapshot: TODO,
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
        );
        
        let duration = start.elapsed();
        tracing::debug!("Created new Deno `MainWorker` in {:?}", duration);
        
        Ok(worker)
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
    pub fn send_broadcast(&self, name: &str, data: &[u8]) -> Result<bool, AbyRuntimeError> {
        let resource = self.broadcast_channel.subscribe()?;
        
        self.async_executor.block_on(async {
            let name = name.to_owned();
            let data = Vec::from(data);
            
            if let Err(error) = self.broadcast_channel.send(&resource, name, data).await {
                return Err(AbyRuntimeError::FailedBroadcastSend(error));
            }
            
            Ok(true)
        })
    }
    
    /// Executes a given module by specifier.
    /// 
    /// Fails when the specified module can't be resolved.
    async fn exec(&self, exec_module_specifier: &str) -> Result<bool, AbyRuntimeError> {
        #[cfg(feature = "dev")]
        tracing::debug!("Executing Module: {:}", exec_module_specifier);
        
        let exec_module_url = self.resolve_module_url(exec_module_specifier)?;
        let mut worker = self.create_worker()?;
        
        worker.execute_main_module(&exec_module_url).await?;
        worker.run_event_loop(false).await?;
        
        // TODO: Collect (and return) run report ..
        Ok(true)
    }
    
    /// TODO
    pub(crate) fn exec_sync(&self, exec_module_specifier: &str) -> Result<bool, AbyRuntimeError> {
        self.async_executor.block_on(self.exec(exec_module_specifier))
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
    #[msg("logging setup failed: {0:}")]
    LoggingSetupFailed(LoggingError),
    
    //--
    /// TODO
    #[msg("invalid runtime config: {0:}")]
    InvalidConfig(AbyRuntimeConfigError),
    
    /// The runtime detected a current or future invalid atomic state.
    #[msg("invalid state '{0:}'")]
    InvalidState(u32),
    
    /// TODO
    #[msg("invalid module specifier '{0:}'")]
    InvalidModuleSpecifier(ModuleResolutionError),
    
    //--
    /// TODO
    #[msg("failed module resolution: {0:}")]
    FailedModuleResolution(ModuleResolutionError),
    
    /// TODO
    #[msg("failed to send broadcast: {0:}")]
    FailedBroadcastSend(AnyError),
    
    /// TODO
    #[msg("io error; {0:}")]
    IoError(std::io::Error),
    
    /// TODO: Move this one to CAbyRuntimeError.
    #[msg("invalid main module '{0:}'")]
    InvalidCBinding(cwrap::error::CBindingError),
    
    //--
    /// TODO
    #[msg("unbeknownst error: {0:}")]
    AnyError(AnyError),
}

impl From<AbyRuntimeConfigError> for AbyRuntimeError {
    /// TODO
    fn from(error: AbyRuntimeConfigError) -> Self {
        AbyRuntimeError::InvalidConfig(error)
    }
}

impl From<cwrap::error::CBindingError> for AbyRuntimeError {
    /// TODO
    fn from(error: cwrap::error::CBindingError) -> Self {
        AbyRuntimeError::InvalidCBinding(error)
    }
}

impl From<cwrap::error::CStringError> for AbyRuntimeError {
    /// TODO
    fn from(error: cwrap::error::CStringError) -> Self {
        AbyRuntimeError::from(cwrap::error::CBindingError::CStringError(error))
    }
}

impl From<std::ffi::NulError> for AbyRuntimeError {
    /// TODO
    fn from(error: std::ffi::NulError) -> Self {
        AbyRuntimeError::InvalidCBinding(cwrap::error::CBindingError::from(error))
    }
}

impl From<ModuleResolutionError> for AbyRuntimeError {
    /// TODO
    fn from(error: ModuleResolutionError) -> Self {
        AbyRuntimeError::FailedModuleResolution(error)
    }
}

impl From<std::io::Error> for AbyRuntimeError {
    /// TODO
    fn from(error: std::io::Error) -> Self {
        AbyRuntimeError::IoError(error)
    }
}

impl From<AnyError> for AbyRuntimeError {
    /// TODO
    fn from(error: AnyError) -> Self {
        AbyRuntimeError::AnyError(error)
    }
}
