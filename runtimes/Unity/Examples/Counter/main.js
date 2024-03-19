import { CounterService } from "./service.js";

console.log("Hello, from main.js!");
try
{
    const service = new CounterService();
    
    await service.start();
    await service.serve();
}
catch (exc)
{
    console.log("Caught exception:", exc);
}