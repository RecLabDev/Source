console.log("Hello, from main.js!");
try
{
    const kv = await Deno.openKv();

    // TODO: Only set this if we need it.
    await kv.set(["counter"], new Deno.KvU64(0n));
    
    // TODO: Get the port from the kv store.
    // const portSetting = await kv.get(["server", "port"]);
    const apiPortSetting = 8000;
    const adminPortSetting = 9000;
    
    const controller = new AbortController();
    
    const apiOptions = {
        port: apiPortSetting,
        signal: controller.signal,
    };
    
    Deno.serve(apiOptions, _request =>
    {
        return new Response("Hello! <3");
    });
    
    const adminOptions = {
        port: adminPortSetting,
        signal: controller.signal,
    };

    Deno.serve(adminOptions, async request =>
    {
        const requestURL = new URL(request.url);
        
        console.debug("Handling request:", request.method, requestURL.pathname);
        
        switch (requestURL.pathname)
        {
            case "/quit":
            {
                controller.abort();
                break;
            }
            case "/exit":
            {
                Deno.exit(0);
                break;
            }
            case "/restart":
            {
                Deno.exit(100);
                break;
            }
            default:
            {
                const countValue = await kv.get(["counter"]);
                const nextValue = countValue.value + 1n;
                
                await kv.set(["counter"], new Deno.KvU64(nextValue));
                console.debug("Incremented Counter to", nextValue);
                
                return new Response("Hello, Admin! <3");
            }
        }
    });
}
catch (exc)
{
    console.log("Caught exception:", exc);
}