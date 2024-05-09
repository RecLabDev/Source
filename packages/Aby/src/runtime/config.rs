use std::path::Path;
use std::path::PathBuf;

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
    inspector_name: Option<String>,
    
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
    pub fn with_inspector_name<V: Into<String>>(mut self, value: V) -> Self {
        self.inspector_name = Some(value.into());
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

impl AbyRuntimeConfig {
    /// TODO
    pub const DEFAULT_INSPECTOR_SOCKET_ADDR: &'static str = "127.0.0.1:9222";

    /// TODO
    pub fn main_module_specifier(&self) -> Option<&str> {
        self.main_module_specifier.as_deref()
    }
    
    /// TODO
    pub fn root_dir(&self) -> Option<&Path> {
        self.root_dir.as_deref()
    }
    
    /// TODO
    pub fn db_dir(&self) -> Option<&Path> {
        self.db_dir.as_deref()
    }
    
    /// TODO
    pub fn log_dir(&self) -> Option<&Path> {
        self.log_dir.as_deref()
    }
    
    /// TODO
    pub fn inspector_name(&self) -> Option<&str> {
        self.inspector_name.as_deref()
    }
    
    /// TODO
    pub fn inspector_addr(&self) -> &str {
        self.inspector_addr.as_deref()
            .unwrap_or(Self::DEFAULT_INSPECTOR_SOCKET_ADDR)
    }
    
    /// TODO
    pub fn inspector_wait(&self) -> bool {
        self.inspector_wait
    }
    
    /// TODO
    pub fn unstable_deno_features(&self) -> &[i32] {
        self.unstable_deno_features.as_ref()
    }
}
