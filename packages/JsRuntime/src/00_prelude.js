// deno-lint-ignore-file
// export * from "ext:runtime/90_deno_ns.js";

// import * as debug from "ext:aby_sdk/01_debug.js";

// Copyright 2018-2024 the Deno authors. All rights reserved. MIT license.
const abyNs = {
    "send_host_log": (message) => console.warning("Not implemented! Message:", message), // Deno[Deno.internal].core.ops.op_send_host_log,
};

// console.debug("Inside Aby JavaScript Runtime!");

export default abyNs;
