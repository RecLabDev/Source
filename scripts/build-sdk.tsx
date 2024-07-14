import * as env from "./.common/env.tsx";
import * as cmd from "./.common/cmd.tsx";

const BUILD_TARGET = "Release";

const STATUS_EXIT_OK = 0;
const STATUS_GENERAL_FAILURE = 1;

try
{
    Deno.chdir("./sdk/DotNet");
    
    const store = Deno.openKv();
    const buildCmd = Deno.run({
        cmd: ["dotnet", "build", "-c", BUILD_TARGET],
        cwd: Deno.cwd(),
    });
    
    console.debug(`Working Dir:`, Deno.cwd());
    console.debug(`Running Command:`, buildCmd);
    
    const buildOutput = buildCmd.output();
    if (buildOutput.code != 0)
    {
        console.error(`Build Failed with exit code ${buildOutput.code}:`, buildOutput.stderr);
        Deno.exit(buildOutput.code); // Forward exit failure reason ..
    }
    
    console.debug(`Built Theta SDK (.NET):\n`, buildOutput.stdout);
    Deno.exit(STATUS_EXIT_OK);
}
catch (exc: any)
{
    console.error(`Whoops!`, exc);
    Deno.exit(STATUS_GENERAL_FAILURE);
}
