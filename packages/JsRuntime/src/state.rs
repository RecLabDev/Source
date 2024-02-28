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
