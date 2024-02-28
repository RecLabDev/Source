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
    #state = undefined;
    
    /**
     * TODO
     */
    #controller = new AbortController();
    
    /**
     * TODO
     */
    #apiOptions = {
        port: DEFAULT_PUBLIC_PORT,
        signal: undefined,
    };
    
    /**
     * TODO
     */
    #adminOptions = {
        port: DEFAULT_ADMIN_PORT,
        signal: undefined,
    };
    
    /**
     * Default CTOR
     */
    constructor()
    {
        this.#apiOptions.signal = this.#controller.signal;
        this.#adminOptions.signal = this.#controller.signal;
    }
    
    /**
     * TODO
     */
    async start()
    {
        this.#state = await Deno.openKv();
    }
    
    /**
     * TODO
     */
    async serve()
    {
        // TODO: Only set this if we need it.
        await this.#state.set(VISIT_COUNTER_KEY, new Deno.KvU64(0n));
        
        Deno.serve(this.#apiOptions, this.serveAPIRequest.bind(this));
        Deno.serve(this.#adminOptions, this.serveAdminRequest.bind(this));
    }
    
    /**
     * TODO
     */
    async serveAPIRequest(request)
    {
        const requestURL = new URL(request.url);
        console.debug("Handling admin request:", request.method, requestURL.pathname);
        
        switch (requestURL.pathname)
        {
            case "/favicon.ico":
            {
                break;
            }
            default:
            {
                const visitorCounter = await this.#state.get(VISIT_COUNTER_KEY);
                const currentVisitorCount = visitorCounter.value + 1n;
                
                await this.#state.set(VISIT_COUNTER_KEY, new Deno.KvU64(currentVisitorCount));
                console.debug("Incremented Counter to", currentVisitorCount);
                
                return new Response(`Hello! You are visitor #${currentVisitorCount}! <3`);
            }
        }
    }
    
    /**
     * TODO
     */
    async serveAdminRequest(request)
    {
        const requestURL = new URL(request.url);
        console.debug("Handling admin request:", request.method, requestURL.pathname);
        
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
                const visitorCounter = await this.#state.get(VISIT_COUNTER_KEY);
                return new Response(`Hello, Admin! There have been ${visitorCounter.value} visitors. <3`);
            }
        }
    }
}
