use deno_runtime::deno_core::error::AnyError;
use deno_runtime::deno_core::*;

/// TODO
pub struct ThetaModuleLoader;

impl ModuleLoader for ThetaModuleLoader {
    /// TODO
    fn resolve(&self, specifier: &str, referrer: &str, _resolution_kind: ResolutionKind) -> Result<ModuleSpecifier, AnyError> {
        // Resolve the module specifier to an absolute URL or path
        deno_runtime::deno_core::resolve_import(specifier, referrer).map_err(|e| e.into())
    }
    
    /// TODO
    fn load(&self, module_specifier: &ModuleSpecifier, _: Option<&ModuleSpecifier>, _: bool, _: RequestedModuleType) -> ModuleLoadResponse {
        let module_specifier = module_specifier.to_owned();
        let module_type = ModuleType::JavaScript;
        let module_code = ModuleSourceCode::String(ModuleCodeString::from_static("console.log('Hello from the module!');"));
        // let code_cache = None; // TODO: This.
        
        println!("Loading module {:}", module_specifier);
        
        ModuleLoadResponse::Async(Box::pin(async move {
            Ok(ModuleSource::new(module_type, module_code, &module_specifier)) // , code_cache))
        }))
    }
}
