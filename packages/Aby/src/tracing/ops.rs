/// TODO
#[deno_runtime::deno_core::op2(fast)]
pub fn op_send_host_log(#[string] message: &str) {
    tracing::trace!("[Host]: {:}", message);
}

/// TODO
#[deno_runtime::deno_core::op2(async)]
#[serde] /// TODO: Can we remove this?
pub async fn op_send_host_log_async(
    // #[string] message: &str
) {
    tracing::trace!("[Host(Async)]: TODO");
}

/// TODO
#[deno_runtime::deno_core::op2(fast)]
pub fn aby_debug(#[string] input: &str) {
    println!("Debugging {}!", input);
}

// deno_runtime::deno_core::extension!(
//     theta_debug_ext,
//     ops = [theta_debug],
//     // esm_entry_point = "ext:theta_debug/bootstrap.js",
//     // esm = [dir "examples/extension_with_esm", "bootstrap.js"]
// );
