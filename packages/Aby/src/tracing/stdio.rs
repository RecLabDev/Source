use std::fs::File;

use deno_runtime::deno_io::Stdio as DenoStdio;
use deno_runtime::deno_io::StdioPipe as DenoStdioPipe;

// #[repr(C)]
pub struct AbyRuntimeTracer {
    stdin: File,
    stdout: File,
    stderr: File,
}

impl AbyRuntimeTracer {
    pub fn try_new(stdout: Option<File>, stderr: Option<File>) -> Result<Self, std::io::Error> {
        Ok(AbyRuntimeTracer {
            stdin: tempfile::tempfile()?,
            stdout: match stdout {
                Some(file) => file,
                None => deno_runtime::deno_io::STDOUT_HANDLE.try_clone()?,
            },
            stderr: match stderr {
                Some(file) => file,
                None => deno_runtime::deno_io::STDERR_HANDLE.try_clone()?,
            },
        })
    }
}

impl AbyRuntimeTracer {
    /// Turn an instance of JsRuntimeStdio into a deno_runtime::io::Stdio,
    /// by cloning the inner file handles.
    ///
    /// TODO: This shoiuld probably be a `try_clone_into()`
    pub fn try_clone_into(&self) -> Result<DenoStdio, std::io::Error> {
        Ok(DenoStdio {
            stdin: DenoStdioPipe::File(self.stdin.try_clone()?),
            stdout: DenoStdioPipe::File(self.stdout.try_clone()?),
            stderr: DenoStdioPipe::File(self.stderr.try_clone()?),
        })
    }
}
