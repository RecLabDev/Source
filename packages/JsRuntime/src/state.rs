// use std::sync::atomic::AtomicU32;
// use std::sync::atomic::Ordering;

use crate::runtime::JsRuntimeError;

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
    
    /// The runtime failed in a predictable way. The host is free to attempt
    /// to recover. Otherwise, shut down gracefully.
    Failed = 3,
    
    /// The runtime encountered an unrecoverable error. The runtime should
    /// shutdown completely before trying again or bad things can happen.
    Panic = 4,
    
    /// The runtime has quit for some reason.
    Shutdown = 5,
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
