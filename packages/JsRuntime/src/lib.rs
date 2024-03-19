#![feature(try_trait_v2)]

pub mod logging;

pub mod tracing;

pub mod runtime;

pub mod event;

pub mod stdio;

pub mod loader;

pub mod bootstrap;

pub mod start;

pub mod cwrap {
    use core::ffi::CStr;
    use core::str::Utf8Error;

    pub enum CStringError {
        Uninitialized,
        #[allow(unused)] // TODO
        Utf8Error(Utf8Error),
    }
    
    pub unsafe fn try_unwrap_cstr<'out>(bytes: *const i8) -> Result<&'out str, CStringError> {
        if bytes.is_null() {
            return Err(CStringError::Uninitialized);
        }
        
        match CStr::from_ptr(bytes).to_str() {
            Ok(c_str) => Ok(c_str),
            Err(error) => Err(CStringError::Utf8Error(error)),
        }
    }
}