#![allow(unused)]

use std::ffi::CString;
use std::net::SocketAddr;
use std::path::Path;
use std::path::PathBuf;
use std::rc::Rc;
use std::result;
use std::sync::atomic::AtomicU32;
use std::sync::atomic::Ordering;
use std::sync::Arc;
use std::sync::Mutex;

// use tokio::runtime::Runtime as TokioRuntime;
use tokio::runtime::Builder as TokioRuntimeBuilder;
use tokio::sync::broadcast;

use cwrap::error::CStringError;

use deno_runtime::BootstrapOptions;
use deno_runtime::UNSTABLE_GRANULAR_FLAGS;
use deno_runtime::worker::MainWorker;
use deno_runtime::worker::WorkerOptions;
use deno_runtime::permissions::PermissionsContainer;
use deno_runtime::inspector_server::InspectorServer;
use deno_runtime::deno_core::resolve_url_or_path;
use deno_runtime::deno_core::FeatureChecker;
use deno_runtime::deno_core::FsModuleLoader;
use deno_runtime::deno_core::ModuleResolutionError;
use deno_runtime::deno_core::ModuleSpecifier;
use deno_runtime::deno_io::Stdio;
use deno_runtime::deno_io::StdioPipe;
use deno_runtime::deno_broadcast_channel::BroadcastChannel;
use deno_runtime::deno_broadcast_channel::InMemoryBroadcastChannel;

use crate::runtime::config::AbyRuntimeConfig;
use crate::runtime::state::AbyRuntimeState;
use crate::runtime::AbyRuntime;
use crate::runtime::AbyRuntimeError;
use crate::tracing::ffi::CJsRuntimeLogLevel;
use crate::tracing::ffi::CLogCallback;

// /// TODO: Probably remove this.
// ///
// /// Uses `OnceLock` for lazy init and lock, `Arc` for sharing,
// /// and `Mutex` for inner mutability.
// pub(crate) static TRACING_DATA: OnceLock<Arc<Mutex<CryptKeeper>>> = OnceLock::new();

/// TODO
#[repr(C)]
#[derive(Debug)]
pub struct CAbyRuntime {
    /// TODO
    pub config: CAbyRuntimeConfig,

    /// TODO
    pub status: CAbyRuntimeStatus,

    /// TODO: Drop this when we're finished with it!!!
    pub ptr: *mut core::ffi::c_void,
}

#[automatically_derived]
impl CAbyRuntime {
    /// Creates a new `CAbyRuntime` assuming ownership and management
    /// of the provided `AbyRuntime` pointer.
    pub fn new(config: CAbyRuntimeConfig, runtime: Box<AbyRuntime>) -> Self {
        let status = CAbyRuntimeStatus::default();
        let ptr = Box::into_raw(runtime) as *mut core::ffi::c_void;

        CAbyRuntime {
            config,
            status,
            ptr,
        }
    }

    /// Provides a safe reference to the `AbyRuntime` if the pointer
    /// is not null, otherwise returns `None`.
    pub fn as_ref(&self) -> Option<&AbyRuntime> {
        unsafe { (self.ptr as *mut AbyRuntime).as_ref() }
    }

    /// Provides a safe mutable reference to the `AbyRuntime` if the
    /// pointer is not null.
    pub fn as_mut(&mut self) -> Option<&mut AbyRuntime> {
        unsafe { (self.ptr as *mut AbyRuntime).as_mut() }
    }
}

#[automatically_derived]
impl core::ops::Deref for CAbyRuntime {
    type Target = AbyRuntime;

    /// TODO
    ///
    /// **Panics if the pointer is null!**
    fn deref(&self) -> &Self::Target {
        self.as_ref().expect("AbyRuntime pointer is null")
    }
}

#[automatically_derived]
impl core::ops::DerefMut for CAbyRuntime {
    /// TODO
    ///
    /// **Panics if the pointer is null!**
    fn deref_mut(&mut self) -> &mut Self::Target {
        self.as_mut().expect("AbyRuntime pointer is null")
    }
}

#[automatically_derived]
impl cwrap::drop::DropExtern for CAbyRuntime {
    /// TODO
    fn drop(self: Box<Self>) {
        #[cfg(feature = "debug")]
        tracing::debug!("Dropping AbyRuntime pointer.");

        unsafe {
            let _ = Box::from_raw(self.ptr as *mut AbyRuntime);
        }
    }
}

/// TODO
#[repr(C)]
#[derive(Clone, Debug)]
pub struct CAbyRuntimeConfig {
    /// TODO
    pub root_dir: *const core::ffi::c_char,

    /// TODO
    pub main_module_specifier: *const core::ffi::c_char,

    /// TODO
    pub db_dir: *const core::ffi::c_char,

    /// TODO
    pub log_dir: *const core::ffi::c_char,

    /// TODO
    pub log_level: CJsRuntimeLogLevel,

    /// TODO
    pub log_callback_fn: CLogCallback,

    /// TODO
    pub inspector_name: *const core::ffi::c_char,

    /// TODO
    pub inspector_addr: *const core::ffi::c_char,

    /// TODO
    pub inspector_wait: bool,
}

impl CAbyRuntimeConfig {
    pub fn new() -> Self {
        CAbyRuntimeConfig::default()
    }
}

impl Default for CAbyRuntimeConfig {
    /// TODO
    fn default() -> Self {
        CAbyRuntimeConfig {
            root_dir: core::ptr::null(),
            main_module_specifier: core::ptr::null(),
            db_dir: core::ptr::null(),
            log_dir: core::ptr::null(),
            log_level: CJsRuntimeLogLevel::Info,
            log_callback_fn: default_log_callback,
            inspector_name: core::ptr::null(),
            inspector_addr: core::ptr::null(),
            inspector_wait: false,
        }
    }
}

// TODO: Move this to the ffi mod.
#[cfg(feature = "ffi")]
impl TryFrom<&crate::runtime::ffi::CAbyRuntimeConfig> for AbyRuntimeConfig {
    type Error = AbyRuntimeError;

    /// TODO
    fn try_from(
        c_runtime_config: &crate::runtime::ffi::CAbyRuntimeConfig,
    ) -> Result<Self, Self::Error> {
        use cwrap::string::try_unwrap_cstr;

        let main_module_specifier = try_unwrap_cstr(c_runtime_config.main_module_specifier)?;
        let root_dir = try_unwrap_cstr(c_runtime_config.root_dir)?;

        let config = AbyRuntimeConfig::new()
            .with_main_module_specifier(main_module_specifier)
            .with_root_dir(root_dir)
            .with_db_dir(try_unwrap_cstr(c_runtime_config.db_dir)?)
            .with_log_dir(try_unwrap_cstr(c_runtime_config.log_dir)?)
            .with_inspector_name(try_unwrap_cstr(c_runtime_config.inspector_name)?)
            .with_inspector_addr(try_unwrap_cstr(c_runtime_config.inspector_addr)?)
            .with_inspector_wait(c_runtime_config.inspector_wait)
            .with_unstable_deno_features({
                // TODO: Get this from `CAbyRuntimeConfig` ..
                UNSTABLE_GRANULAR_FLAGS
                    .iter()
                    .map(|&feature| feature.2)
                    .collect::<Vec<i32>>()
            });

        Ok(config)
    }
}

// impl CAbyRuntimeConfig {
//     /// TODO
//     pub unsafe fn get_main_module_specifier(&self) -> Result<ModuleSpecifier, AbyRuntimeError> {
//         let main_module_specifier = cwrap::string::try_unwrap_cstr(self.main_module_specifier)?;
//         let root_dir = PathBuf::from(cwrap::string::try_unwrap_cstr(self.root_dir)?);
//         Ok(resolve_url_or_path(main_module_specifier, &root_dir)?)
//     }
// }

//---
/// TODO
#[repr(C)]
#[derive(Debug)]
pub struct CConstructRuntimeResult {
    /// TODO
    pub code: CConstructRuntimeResultCode,
    
    /// TODO
    pub runtime: *mut CAbyRuntime,
}

#[automatically_derived]
impl CConstructRuntimeResult {
    /// TODO
    pub fn new(code: CConstructRuntimeResultCode, maybe_runtime: Option<CAbyRuntime>) -> Self {
        let runtime = maybe_runtime
            .map(|runtime| Box::into_raw(Box::new(runtime)))
            .unwrap_or(core::ptr::null_mut());

        CConstructRuntimeResult { code, runtime }
    }
}

impl From<AbyRuntimeError> for CConstructRuntimeResult {
    /// TODO
    fn from(error: AbyRuntimeError) -> Self {
        CConstructRuntimeResult::new(CConstructRuntimeResultCode::from(error), None)
    }
}

/// TODO
#[repr(C)]
#[derive(Debug, PartialEq)]
pub enum CConstructRuntimeResultCode {
    /// All operations completed successfully.
    Ok,
    
    /// TODO
    InvalidConfig,
    
    /// TODO
    InvalidDataDir,
    
    /// TODO
    InvalidLogDir,
    
    /// TODO
    InvalidMainModule,
    
    /// TODO
    InvalidBindings,
    
    /// TODO
    FailedSetup,
    
    /// TODO
    FailedOperation,
    
    /// TODO
    FailedBroadcast,
}

impl From<CConstructRuntimeResultCode> for CConstructRuntimeResult {
    /// Get a `CConstructRuntimeResult` from a `CConstructRuntimeError`.
    fn from(code: CConstructRuntimeResultCode) -> Self {
        CConstructRuntimeResult {
            code,
            runtime: core::ptr::null_mut(),
        }
    }
}

impl From<AbyRuntimeError> for CConstructRuntimeResultCode {
    /// TODO
    fn from(error: AbyRuntimeError) -> Self {
        match error {
            AbyRuntimeError::Uninitialized => CConstructRuntimeResultCode::FailedSetup,
            AbyRuntimeError::FailedModuleResolution(_) => CConstructRuntimeResultCode::InvalidMainModule,
            AbyRuntimeError::FailedBroadcastSend(_) => CConstructRuntimeResultCode::FailedBroadcast,
            AbyRuntimeError::InvalidConfig(_) => CConstructRuntimeResultCode::InvalidConfig,
            AbyRuntimeError::InvalidModuleSpecifier(_) => CConstructRuntimeResultCode::InvalidConfig,
            AbyRuntimeError::InvalidState(_) => CConstructRuntimeResultCode::InvalidConfig,
            AbyRuntimeError::InvalidCBinding(_) => CConstructRuntimeResultCode::InvalidBindings,
            AbyRuntimeError::LoggingSetupFailed(_) => CConstructRuntimeResultCode::FailedSetup,
            AbyRuntimeError::IoError(_) => CConstructRuntimeResultCode::FailedOperation,
            AbyRuntimeError::AnyError(_) => CConstructRuntimeResultCode::InvalidConfig,
        }
    }
}

type CErrorReportFn = extern "C" fn(message: *const std::ffi::c_char);

/// Construct an instance of AbyRuntime from a c-like boundary.
///
/// ### Example:
/// ```rust
/// let result = aby::runtime::ffi::c_construct_runtime({
///     CAbyRuntimeConfig {
///         // TODO
///     }
/// });
///
/// let status = aby::runtime::ffi::c_exec_module(result.runtime, CExecModuleOptions {
///     // TODO
/// });
/// ```
#[export_name = "aby__c_construct_runtime"]
pub extern "C" fn c_construct_runtime(c_runtime_config: CAbyRuntimeConfig) -> CConstructRuntimeResult {
    // Get a new copy of the target config for the new runtime instance.
    let runtime_config = match AbyRuntimeConfig::try_from(&c_runtime_config) {
        Ok(runtime_config) => runtime_config,
        Err(error) => {
            // TODO: Send trace messages back to Rust via log fn.
            // let c_message = CString::new(format!("Error: {:}", error)).expect("TODO");
            // (c_runtime_config.log_callback_fn)(CJsRuntimeLogLevel::Error, c_message.as_ptr());
            tracing::error!("Failed to get `AbyRuntimeConfig`: {:}", error);
            return CConstructRuntimeResult::from(error);
        }
    };
    
    let aby_runtime = match AbyRuntime::new(runtime_config) {
        Ok(aby_runtime) => aby_runtime,
        Err(error) => {
            // TODO: Send trace messages back to Rust via log fn.
            // let c_message = CString::new(format!("Error: {:}", error)).expect("TODO");
            // (c_runtime_config.log_callback_fn)(CJsRuntimeLogLevel::Error, c_message.as_ptr());
            tracing::error!("Failed to get `AbyRuntimeConfig`: {:}", error);
            return CConstructRuntimeResult::from(error);
        }
    };

    // let aby_runtime = match aby_runtime.do_work() {
    //     Ok(aby_runtime) => {
    //         // TODO
    //     }
    //
    //     Err(AbyRuntimeError::AnyError(error)) => {
    //         //..
    //         return CConstructRuntimeResult::from(CConstructRuntimeError::DataDirInvalidErr)
    //     }
    //
    //     Err(error) => {
    //         //..
    //         return CConstructRuntimeResult::from(CConstructRuntimeError::StdioErr)
    //     }
    // };

    let c_aby_runtime = CAbyRuntime::new(c_runtime_config, Box::new(aby_runtime));
    CConstructRuntimeResult::new(CConstructRuntimeResultCode::Ok, Some(c_aby_runtime))
}

impl CAbyRuntime {
    unsafe fn try_from_mut_ptr<'out>(ptr: *mut CAbyRuntime) -> Option<&'out mut CAbyRuntime> {
        // TODO: Ensure Pointer is safe to use.
        Some(&mut *ptr)
    }

    unsafe fn try_from_ptr<'out>(ptr: *const CAbyRuntime) -> Option<&'out CAbyRuntime> {
        // TODO: Ensure Pointer is safe to use.
        Some(&*ptr)
    }

    pub fn send_host_log<M: Into<String>>(&self, message: M) {
        if let Err(error) = self.try_send_host_log(message) {
            tracing::error!("Failed to send host message: {:}", error);
        }
    }

    pub fn try_send_host_log<M: Into<String>>(&self, message: M) -> Result<bool, std::io::Error> {
        match CString::new(message.into()) {
            Ok(message) => {
                (self.config.log_callback_fn)(CJsRuntimeLogLevel::Debug, message.as_ptr());
                Ok(true) // <3
            }
            Err(error) => Err(std::io::Error::other(format!("TODO: {:}", error))),
        }
    }
}

pub fn cwrap_unpack_byte_slice<'out, T>(data: *const u8, length: usize) -> Option<&'out [u8]> {
    if data.is_null() || length == 0 {
        return None;
    }
    
    let data = unsafe {
        core::slice::from_raw_parts(data, length)
    };
    
    Some(data)
}

pub fn cwrap_unpack_byte_vec<T>(data: *const u8, length: usize) -> Option<Vec<u8>> {
    if data.is_null() || length == 0 {
        return None;
    }
    
    let data = unsafe {
        core::slice::from_raw_parts(data, length)
    };
    
    Some(Vec::from(data))
}

pub fn cwrap_trace_error_with_prefix<E: core::error::Error>(prefix: &'static str) -> impl FnOnce(E) -> E {
    move |error| {
        tracing::error!("{:}: {:}", prefix, error);
        error
    }
}

// TODO: Move this to a shared ffi module ..
pub(crate) fn c_trace_error<E: core::error::Error>(error: E) -> E {
    tracing::error!("Oh Cwrap: {:}", error);
    error
}

//---
#[repr(C)]
#[derive(Debug)]
pub struct CSendBroadcastOptions {
    pub name: *const i8,
    pub data: *const u8,
    pub length: usize,
}

#[allow(unused, unreachable_code)]
#[export_name = "aby__c_send_broadcast"]
pub unsafe extern "C" fn c_send_broadcast(c_aby_runtime_ptr: *const CAbyRuntime, options: CSendBroadcastOptions) {
    let Some(c_aby_runtime) = CAbyRuntime::try_from_ptr(c_aby_runtime_ptr) else {
        return; // CSendBroadcastResult::RuntimeNul
    };

    let Some(aby_runtime) = c_aby_runtime.as_ref() else {
        return; // TODO: CSendBroadcastResult::MissingRuntime
    };

    let Ok(name) = cwrap::try_unwrap_cstr(options.name).map_err(c_trace_error) else {
        return; // TODO: CSendBroadcastResult::InvalidName
    };
    
    let Some(data) = cwrap_unpack_byte_slice::<u8>(options.data, options.length) else {
        return; // TODO: CSendBroadcastResult::InvalidData
    };

    if let Err(error) = aby_runtime.send_broadcast(name, data) {
        tracing::error!("Failed to send broadcast message: {:}", error);
        // TODO: return CSendBroadcastResult::FailedSend;
    }
}

//---
#[repr(C)]
#[derive(Debug)]
pub struct CExecModuleOptions {
    pub module_specifier: *const std::ffi::c_char,
}

/// TODO
#[repr(C)]
#[derive(Debug)]
pub enum CExecModuleResult {
    /// All operations completed successfully.
    Ok,

    /// Failed during binding.
    RuntimeNul,

    /// TODO
    RuntimePanic,

    /// TODO
    FailedCreateAsyncRuntime,

    /// TODO
    FailedFetchingWorkDirErr,

    /// TODO
    DataDirInvalidErr,

    /// TODO
    LogDirInvalidErr,

    /// TODO
    MainModuleInvalidErr,

    /// TODO
    MainModuleUninitializedErr,

    /// TODO
    FailedModuleExecErr,

    /// TODO
    FailedEventLoopErr,
}

use deno_runtime::deno_core::url::Url as ModuleUrl;

/// TODO
#[allow(unused_variables)]
#[export_name = "aby__c_exec_module"]
pub unsafe extern "C" fn c_exec_module(c_aby_runtime_ptr: *mut CAbyRuntime, options: CExecModuleOptions) -> CExecModuleResult {
    let Some(c_aby_runtime) = CAbyRuntime::try_from_mut_ptr(c_aby_runtime_ptr) else {
        return CExecModuleResult::RuntimeNul;
    };
    
    let Ok(exec_module_specifier) = cwrap::try_unwrap_cstr(options.module_specifier) else {
        return CExecModuleResult::MainModuleInvalidErr;
    };
    
    // match c_aby_runtime_ptr.exec_sync(exec_module_specifier) {
    //     Ok(result) => {
    //         // TODO: Report the exec result to the host.
    //         tracing::debug!("Executed module '{:}' with result ({:}) ..", exec_module_specifier, result);
    //         CExecModuleResult::Ok
    //     }
    //     Err(error) => {
    //         tracing::error!("Failed to execute module '{:}': {:}", exec_module_specifier, error);
    //         CExecModuleResult::FailedModuleExecErr
    //     }
    // }
    
    // TODO: Maybe we should be using a panic hook instead?
    // Ref: https://doc.rust-lang.org/std/panic/fn.set_hook.html
    match std::panic::catch_unwind(|| {
        match c_aby_runtime.exec_sync(exec_module_specifier) {
            Ok(exit_status) => {
                tracing::debug!("Exited with status {:}", exit_status);
                CExecModuleResult::Ok // <3
            }
            Err(error) => match error {
                AbyRuntimeError::AnyError(any_error) => {
                    tracing::error!("Runtime exited with error: {:}", any_error);
                    CExecModuleResult::FailedModuleExecErr
                }
                _ => {
                    tracing::error!("Failed to execute module: {:}", error);
                    CExecModuleResult::FailedModuleExecErr
                }
            },
        }
    }) {
        Ok(exit_status) => exit_status,
        Err(panic) => {
            crate::tracing::ffi::handle_panic(panic);
            CExecModuleResult::RuntimePanic
        }
    }
}

#[export_name = "aby__c_free_runtime"]
pub unsafe extern "C" fn c_free_runtime(c_aby_runtime_ptr: *mut CAbyRuntime) {
    let _ = Box::from_raw(c_aby_runtime_ptr);
}

extern "C" fn default_log_callback(level: CJsRuntimeLogLevel, message: *const core::ffi::c_char) {
    match cwrap::string::try_unwrap_cstr(message) {
        Ok(message) => match level {
            CJsRuntimeLogLevel::Error => tracing::error!("{:}", message),
            CJsRuntimeLogLevel::Warning => tracing::warn!("{:}", message),
            CJsRuntimeLogLevel::Info => tracing::info!("{:}", message),
            CJsRuntimeLogLevel::Debug => tracing::debug!("{:}", message),
            CJsRuntimeLogLevel::Trace => tracing::trace!("{:}", message),
            CJsRuntimeLogLevel::None => tracing::debug!("{:}", message),
        },
        Err(error) => {
            tracing::error!("Failed to unwrap incoming log message: {:}", error)
        }
    }
}

//---
/// Represents the state of an active `AbyRuntime` instance.
#[repr(C)]
#[derive(Default, Debug)]
pub enum CAbyRuntimeStatus {
    /// No state has been set, yet. Treat this as "uninitialized".
    #[default]
    None = 0,

    /// Runtime has been bootstrapped but not yet "warm" (running).
    Cold,

    /// The runtime is executing startup operations. Try again next frame.
    Startup,

    /// The runtime is working and has had no problems (yet).
    /// Check later for failures, but all good so far!
    Warm,

    /// The runtime failed in a predictable way. The host is free to
    /// attempt to recover. Otherwise, shut down gracefully.
    Failure,

    /// The runtime encountered an unrecoverable error. The runtime should
    /// shutdown completely before trying again or bad things can happen.
    Panic,

    /// The runtime has quit for some reason.
    Shutdown,
}

impl TryFrom<u32> for CAbyRuntimeStatus {
    /// TODO
    type Error = AbyRuntimeError;

    /// TODO
    fn try_from(value: u32) -> Result<CAbyRuntimeStatus, Self::Error> {
        match value {
            0 => Ok(CAbyRuntimeStatus::None),
            1 => Ok(CAbyRuntimeStatus::Cold),
            2 => Ok(CAbyRuntimeStatus::Startup),
            3 => Ok(CAbyRuntimeStatus::Warm),
            4 => Ok(CAbyRuntimeStatus::Failure),
            5 => Ok(CAbyRuntimeStatus::Shutdown),
            _ => Err(AbyRuntimeError::InvalidState(value)),
        }
    }
}

/// TODO
#[repr(C)]
pub enum CJsRuntimeEventKind {
    Hup = 0,
}
