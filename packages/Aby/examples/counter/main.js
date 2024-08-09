import { CounterService } from "./service.js";

const channel = new BroadcastChannel("RECLAB_MESSAGES");
channel.onmessage = (event) => {
    console.log("Found message:", event);
};

console.log("Hello, from main.js!");
try
{
    // console.debug("Deno Internal Ops:", Deno);
    // Deno.core.opAsync("op_send_host_log", { message: "test" });
    // Deno[Deno.internal].core.ops.op_send_host_log("Omfg ..");
    
    const counterService = new CounterService();
    
    await counterService.start();
    await counterService.serve();
}
catch (exc)
{
    console.error("Caught exception:", exc);
}