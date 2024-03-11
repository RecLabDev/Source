import * as env from "./.common/env.tsx";
import * as cmd from "./.common/cmd.tsx";

const BUILD_TARGET = "Release";

try
{
    const store = Deno.openKv();
    
    let workingDir = Deno.cwd();
    
    console.log(`Current Working Directory: ${workingDir}`);
    
    const buildOutput = await cmd.execSync("dotnet", "build", "-c", BUILD_TARGET);
    if (buildOutput.success == false)
    {
        console.error(`Build Failed: ${buildOutput.code}`, buildOutput.stderr);
        Aby.exit(70);
    }
    
    console.debug(`Built Theta SDK (.NET):\n`, buildOutput.stdout);
}
catch (exc: any)
{
    console.error(`Whoops!`, exc);
}
