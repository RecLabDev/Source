use std::rc::Rc;
use std::sync::Arc;
use std::path::PathBuf;
use std::process::ExitCode;

use anyhow::Result;

use deno_runtime::worker::MainWorker;
use deno_runtime::worker::WorkerOptions;
use deno_runtime::permissions::PermissionsContainer;
use deno_runtime::deno_core::FeatureChecker;
use deno_runtime::deno_core::FsModuleLoader;
use deno_runtime::deno_core::resolve_url_or_path;
use deno_runtime::BootstrapOptions;
use deno_runtime::UNSTABLE_GRANULAR_FLAGS;

//---
/// TODO: Remove `swc_ecma_codegen` when we've resolved the sourcemap error.
const TRACING_FILTER: &str = "aby-basic=trace,aby=trace,swc_ecma_codegen=off,warn";

//---
/// TODO
#[tokio::main(flavor = "current_thread")]
async fn main() -> Result<ExitCode> {
    aby::tracing::mount(TRACING_FILTER);
    
    let working_dir = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    let data_dir = PathBuf::from("./examples/counter/db");
    let module_specifier = "./examples/counter/main.js"; // TODO: From args ..
    let main_module = resolve_url_or_path(module_specifier, &working_dir)?;
    let exec_module = resolve_url_or_path(module_specifier, &working_dir)?;
    
    tracing::info!("Executing '{:}'", exec_module);

    let mut worker = MainWorker::bootstrap_from_options(
        main_module,
        PermissionsContainer::allow_all(),
        WorkerOptions {
            bootstrap: create_bootstrap_options(),
            feature_checker: create_feature_checker(),
            skip_op_registration: false,
            module_loader: Rc::new(FsModuleLoader),
            origin_storage_dir: Some(data_dir),
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

/// TODO
fn create_bootstrap_options() -> BootstrapOptions {
    BootstrapOptions {
        unstable_features: {
            UNSTABLE_GRANULAR_FLAGS.iter()
                .map(|&feature| feature.2)
                .collect()
        },
        ..Default::default()
    }
    
}

/// TODO
fn create_feature_checker() -> Arc<FeatureChecker> {
    let mut feature_checker = FeatureChecker::default();
    
    for feature in UNSTABLE_GRANULAR_FLAGS.iter() {
        feature_checker.enable_feature(feature.0);
    }
    
    Arc::new(feature_checker)
}
