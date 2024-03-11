const VISIT_COUNTER_KEY = ["counter"];

const DEFAULT_PUBLIC_PORT = 8000;

const DEFAULT_ADMIN_PORT = 9000;

/**
 * TODO
 */
export class SomeService
{
    
    
    /**
     * TODO
     */
    #store = undefined;
    
    /**
     * TODO
     */
    #queue = undefined;
    
    /**
     * TODO
     */
    #controller = new AbortController();
    
    /**
     * TODO
     */
    #apiOptions = {
        port: DEFAULT_PUBLIC_PORT,
        signal: this.#controller.signal,
    };
    
    /**
     * TODO
     */
    #adminOptions = {
        port: DEFAULT_ADMIN_PORT,
        signal: this.#controller.signal,
    };
    
    /**
     * Default CTOR
     */
    constructor()
    {
        //..
    }
    
    /**
     * TODO
     */
    async start()
    {
        this.#store = await Deno.openKv();
    }
    
    /**
     * TODO
     */
    // deno-lint-ignore require-await
    async serve()
    {
        try
        {
            Deno.serve(this.#apiOptions, this.handleRestRequest.bind(this));
            Deno.serve(this.#adminOptions, this.handleAdminRequest.bind(this));
        }
        catch (exc)
        {
            console.error("Failed to serve (http):", exc);
        }
    }
    
    /**
     * TODO
     */
    async #incrementCounter(value)
    {
        await this.#store.atomic()
            .mutate({
                type: "sum",
                key: VISIT_COUNTER_KEY,
                value: new Deno.KvU64(value),
            })
            .commit();
        
        return await this.#store.get(VISIT_COUNTER_KEY);
    }
    
    /**
     * TODO
     */
    async handleRestRequest(request)
    {
        const requestURL = new URL(request.url);
        
        switch (requestURL.pathname)
        {
            case "/favicon.ico":
            {
                return new Response(``);
            }
            default:
            {
                const visitorCounter = await this.#incrementCounter(1n);
                console.debug(`Incremented counter to ${visitorCounter.value}`);
                
                return new Response(`Hello! You are visitor #${visitorCounter.value}! <3`);
            }
        }
    }
    
    /**
     * TODO
     */
    async handleAdminRequest(request)
    {
        const requestURL = new URL(request.url);
        
        switch (requestURL.pathname)
        {
            case "/favicon.ico":
            {
                return new Response(``);
            }
            case "/quit":
            {
                this.#controller.abort();
                return new Response(``);
            }
            case "/exit":
            {
                Deno.exit(0);
                return new Response(``);
            }
            case "/restart":
            {
                Deno.exit(100);
                return new Response(``);
            }
            default:
            {
                const visitorCounter = await this.#store.get(VISIT_COUNTER_KEY);
                return new Response(`Hello, Admin! There have been ${visitorCounter.value} visitors. <3`);
            }
        }
    }
}
