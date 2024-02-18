console.log("Hello, from main.js!");

Deno.serve((request) =>
{
    console.log("Handling request:", request.method, request.url);

    return new Response("Hello, world");
});
