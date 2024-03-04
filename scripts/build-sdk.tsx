import * as env from "./.common/env.tsx";
import * as cmd from "./.common/cmd.tsx";

const buildTarget = "Release";

try
{
    const store = Deno.openKv();
    
    console.log(`Current Working Directory: ${Deno.cwd()}`);
    
    const buildOutput = await cmd.execSync("dotnet", "build", "-c", buildTarget);
    if (buildOutput.success == false)
    {
        console.error(`Build Failed: ${buildOutput.code}`, buildOutput.stderr);
    }
    else
    {
        console.debug(`Built Theta SDK (.NET):\n`, buildOutput.stdout);
    }
}
catch (exc: any)
{
    console.error(`Whoops!`, exc);
}
