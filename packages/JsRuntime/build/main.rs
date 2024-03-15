use std::process::ExitCode;

use anyhow::Result;

/// TODO
fn main() -> Result<ExitCode> {
    // Generate Unity-frinedly Bindings
    // #[cfg(feature = "unity")]
    csbindgen::Builder::default()
        .input_extern_file("./src/lib.rs")
        .input_extern_file("./src/config.rs")
        .input_extern_file("./src/bootstrap.rs")
        .input_extern_file("./src/stdio.rs")
        .input_extern_file("./src/loader.rs")
        .input_extern_file("./src/logging.rs")
        .input_extern_file("./src/tracing.rs")
        .input_extern_file("./src/event.rs")
        .input_extern_file("./src/runtime.rs")
        .input_extern_file("./src/start.rs")
        .csharp_dll_name("JsRuntime")
        .csharp_dll_name_if("UNITY_IOS && !UNITY_EDITOR", "__Internal")
        .csharp_file_header("#if !UNITY_WEBGL")
        .csharp_file_footer("#endif")
        .csharp_namespace("Theta.Unity.Runtime")
        .csharp_class_accessibility("public")
        .csharp_class_name("JsRuntime") // TODO: Change to `NativeBindings``
        .csharp_use_function_pointer(false) // TODO: Can we make this true?
        .csharp_type_rename(map_type_names)
        .generate_csharp_file("./gen/Unity/JsRuntime.g.cs")
        .expect("Failed to generate CSharp bindings.");
    
    Ok(ExitCode::SUCCESS)
}

/// TODO
fn map_type_names(rust_type_name: String) -> String {
    match rust_type_name.as_str() {
        _ => rust_type_name,
    }
}