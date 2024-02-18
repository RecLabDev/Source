use std::process::ExitCode;

use anyhow::Result;

/// TODO
const TRACING_FILTER: &str = "js_runtime=trace,info";

//---
fn main() -> Result<ExitCode> {
    // Setup a default tracing subscriber so we can see log output.
    // In a real environment, this would typically be setup during init
    //   with a more robust subscriber which can collect and re-route
    //   tracing events to a host-system.
    tracing_subscriber::fmt()
        .with_env_filter(TRACING_FILTER)
        .with_thread_names(true)
        .with_thread_ids(false)
        .with_target(true)
        .with_file(false)
        .with_timer(true)
        .without_time()
        .init();
    
    // First, run the bootstrap operation to mount resources and otherwise
    //   prepare the process, ffi boundary, etc.
    js_runtime::bootstrap();
    
    // Second, start the global `MainWorker`. Status 0 is good, 1+ is bad.
    let status = unsafe { js_runtime::start(0) };
    tracing::trace!("JsRuntime Exited with status {:}", status);
    
    // Yay! <3
    Ok(ExitCode::SUCCESS)
}
