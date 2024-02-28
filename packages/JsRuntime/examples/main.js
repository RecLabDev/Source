import { SomeService } from "./service.js";

console.log("Hello, from main.js!");
try
{
    const service = new SomeService();
    
    await service.start();
    await service.serve();
}
catch (exc)
{
    console.log("Caught exception:", exc);
}