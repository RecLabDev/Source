use std::process::ExitCode;

use anyhow::Result;

/// TODO
pub fn main() -> Result<ExitCode> {
    // Generate Unity-frinedly Bindings
    #[cfg(feature = "unity")]
    csbindgen::Builder::default()
        .input_extern_file("./src/ffi.rs")
        .csharp_dll_name("JsRuntime")
        .csharp_dll_name_if("UNITY_IOS && !UNITY_EDITOR", "__Internal")
        .csharp_file_header("#if !UNITY_WEBGL")
        .csharp_file_footer("#endif")
        .csharp_namespace("Theta.Unity.Runtime")
        .csharp_class_accessibility("public")
        .csharp_class_name("JsRuntime")
        .csharp_use_function_pointer(false)
        .csharp_type_rename(|rust_type_name| {
            match rust_type_name.as_str() {
                _ => rust_type_name,
            }
        })
        .generate_csharp_file("./gen/Unity/JsRuntime.g.cs")
        .expect("Failed to generate CSharp bindings.");
    
    Ok(ExitCode::SUCCESS)
}
