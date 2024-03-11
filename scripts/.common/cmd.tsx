/**
 * TODO
 */
export async function execSync(cmdString: string, ...args: any[])
{
    const cmd = new Deno.Command(cmdString, {
        args,
        stdout: "piped",
        stderr: "piped",
    });
    
    const cmdProc = cmd.spawn();
    if (cmdProc !== null)
    {
        cmdProc.ref();
    }
    
    // TODO: Use cmdProc's stdout WritableStream to pipe output to stdout.
    
    // TODO: Wait for proc to finish.
    
    return await cmdProc.output()
}
