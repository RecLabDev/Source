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
    async handleRestRequest(request)
    {
        const requestURL = new URL(request.url);
        console.debug("Handling api request:", request.method, requestURL.pathname);
        
        switch (requestURL.pathname)
        {
            case "/favicon.ico":
            {
                return new Response();
            }
            default:
            {
                const visitorCounter = await this.#store.get(VISIT_COUNTER_KEY, 0);
                const currentVisitorCount = visitorCounter.value + 1n;
                
                await this.#store.set(VISIT_COUNTER_KEY, new Deno.KvU64(currentVisitorCount));
                console.debug("Incremented Counter to", currentVisitorCount);
                
                return new Response(`Hello! You are visitor #${currentVisitorCount}! <3`);
            }
        }
    }
    
    /**
     * TODO
     */
    async handleAdminRequest(request)
    {
        const requestURL = new URL(request.url);
        console.debug("Handling admin request:", request.method, requestURL.pathname);
        
        switch (requestURL.pathname)
        {
            case "/favicon.ico":
            {
                return new Response();
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
