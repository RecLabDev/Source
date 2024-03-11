use std::process::ExitCode;

use anyhow::Result;

/// TODO
fn main() -> Result<ExitCode> {
    // TODO
    csbindgen::Builder::default()
        .input_extern_file("ffi.rs")
        .csharp_dll_name("JsRuntimeCSharp")
        .generate_csharp_file("./NativeMethods.g.cs")
        .expect("Failed to generate CSharp bindings.");
}
