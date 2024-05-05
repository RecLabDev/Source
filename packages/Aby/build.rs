use std::process::ExitCode;

use anyhow::Result;

/// TODO
fn main() -> Result<ExitCode> {
    // Generate Unity-frinedly Bindings
    #[cfg(feature = "unity")]
    csbindgen::Builder::default()
        .input_extern_file("./src/tracing/ffi.rs")
        .input_extern_file("./src/runtime/ffi.rs")
        .csharp_dll_name("AbyRuntime")
        .csharp_dll_name_if("UNITY_IOS && !UNITY_EDITOR", "__Internal")
        .csharp_file_header("#if !UNITY_WEBGL")
        .csharp_file_footer("#endif")
        .csharp_namespace("Aby.Unity.Plugin")
        .csharp_class_accessibility("public")
        .csharp_class_name("NativeMethods") // TODO: Change to `NativeBindings``
        .csharp_use_function_pointer(false) // TODO: Can we make this true?
        .csharp_type_rename(map_unity_type_names)
        .generate_csharp_file("./gen/Unity/AbyRuntime.g.cs")
        .expect("Failed to generate CSharp bindings");
    
    Ok(ExitCode::SUCCESS)
}

/// TODO
#[cfg(feature = "unity")]
fn map_unity_type_names(rust_type_name: String) -> String {
    match rust_type_name.as_str() {
        _ => rust_type_name,
    }
}