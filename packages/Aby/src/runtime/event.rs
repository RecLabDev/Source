#[cfg(feature="ffi")]
pub mod ffi {
    /// TODO
    #[repr(C)]
    pub enum CJsRuntimeEventKind {
        Hup = 0,
    }
}