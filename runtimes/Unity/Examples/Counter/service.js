const VISIT_COUNTER_KEY = ["counter"];

const DEFAULT_PUBLIC_PORT = 10000;

const DEFAULT_ADMIN_PORT = 11000;

/**
 * TODO
 */
export class CounterService
{
    /**
     * TODO
     */
    #store = undefined;
    
    /**
     * TODO
     */
    #channel = new BroadcastChannel("all_messages");
    
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
        this.#channel.onmessage = this.#onBroadcastMessage.bind(this);
        this.#channel.onmessageerror = this.#onBroadcastError.bind(this);
    }
    
    #onBroadcastMessage(event)
    {
        console.debug(`Got broadcast message:`, event);
        if (event.quit)
        {
            console.log(`Got quit message. Attempting safe shutdown ..`);
            this.#controller.abort();
        }
    }
    
    #onBroadcastError(event)
    {
        console.error(`Got broadcast error:`, event);
    }
    
    /**
     * TODO
     */
    async start()
    {
        this.#store = await Deno.openKv();
        this.#store.listenQueue(this.#onQueueReceive.bind(this));
    }
    
    async #onQueueReceive(message)
    {
        console.log("Got message:", message);
        this.#controller.abort();
    }
    
    /**
     * TODO
     */
    // deno-lint-ignore require-await
    async serve()
    {
        try
        {
            Deno.serve(this.#apiOptions, this.handleApiRequest.bind(this));
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
    async handleApiRequest(request)
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
                const message = { quit: true };
                const result = this.#channel.postMessage(message);
                
                this.#store.enqueue({ channel: "C123456", text: "Slack message" }, {
                    delay: 1, // Seconds?
                });
                
                // Note: The broadcast message isn't collected locally.
                console.log("Posted message:", message);
                console.debug("Broadcast Result:", result);
                console.debug("Channel:", this.#channel);
                
                return new Response(`Quitting`);
            }
            case "/exit":
            {
                Deno.exit(0);
                return new Response(`Exiting`);
            }
            case "/restart":
            {
                Deno.exit(100);
                return new Response(`Restarting`);
            }
            default:
            {
                const visitorCounter = await this.#store.get(VISIT_COUNTER_KEY);
                return new Response(`Hello, Admin! There have been ${visitorCounter.value} visitors. <3`);
            }
        }
    }
}
