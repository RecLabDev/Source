use deno_runtime::deno_core::op2;

//---
/// TODO
#[op2(fast)]
pub fn theta_debug(#[string] input: &str) {
    println!("Debugging {}!", input);
}

// deno_runtime::deno_core::extension!(
//     theta_debug_ext,
//     ops = [theta_debug],
//     // esm_entry_point = "ext:theta_debug/bootstrap.js",
//     // esm = [dir "examples/extension_with_esm", "bootstrap.js"]
// );
