use std::process::ExitCode;
use std::path::Path;
use std::rc::Rc;

use anyhow::Result;

use deno_runtime::deno_core::FsModuleLoader;
use deno_runtime::deno_core::ModuleSpecifier;
use deno_runtime::permissions::PermissionsContainer;
use deno_runtime::worker::MainWorker;
use deno_runtime::worker::WorkerOptions;

#[tokio::main(flavor = "current_thread")]
async fn main() -> Result<ExitCode> {
    let js_path = Path::new(env!("CARGO_MANIFEST_DIR")).join("examples/main.js");
    let main_module = ModuleSpecifier::from_file_path(js_path).unwrap();
    let mut worker = MainWorker::bootstrap_from_options(
        main_module.clone(),
        PermissionsContainer::allow_all(),
        WorkerOptions {
            module_loader: Rc::new(FsModuleLoader),
            extensions: vec![],
            ..Default::default()
        },
    );
    
    worker.execute_main_module(&main_module).await?;
    worker.run_event_loop(false).await?;
    
    Ok(ExitCode::SUCCESS)
}