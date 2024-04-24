use std::process::ExitCode;
use std::path::Path;
use std::rc::Rc;

use anyhow::Result;

use deno_runtime::worker::MainWorker;
use deno_runtime::worker::WorkerOptions;
use deno_runtime::permissions::PermissionsContainer;
use deno_runtime::deno_core::FsModuleLoader;
use deno_runtime::deno_core::resolve_url_or_path;

//---
/// TODO: Remove `swc_ecma_codegen` when we've resolved the sourcemap error.
const TRACING_FILTER: &str = "aby-basic=trace,aby=trace,swc_ecma_codegen=off,warn";

//---
/// TODO
#[tokio::main(flavor = "current_thread")]
async fn main() -> Result<ExitCode> {
    aby::tracing::mount(TRACING_FILTER);
    
    let working_dir = Path::new(env!("CARGO_MANIFEST_DIR"));
    let module_specifier = "./examples/basic/main.tsx"; // TODO: From args ..
    let main_module = resolve_url_or_path(module_specifier, working_dir)?;
    let exec_module = main_module.clone();
    
    tracing::info!("Executing '{:}'", main_module);

    let mut worker = MainWorker::bootstrap_from_options(
        main_module,
        PermissionsContainer::allow_all(),
        WorkerOptions {
            module_loader: Rc::new(FsModuleLoader),
            extensions: vec![
                //..
            ],
            ..Default::default()
        },
    );
    
    worker.execute_main_module(&exec_module).await?;
    worker.run_event_loop(false).await?;
    
    Ok(ExitCode::SUCCESS)
}