//using Spectre.Console;

using spinner;

using App app = new(string.Join(" ", args));
var podman = new PodmanService();

try
{
    CLICommandOutput ready = app.Init();

    if (ready.Success)
    {
        await app.Start();
    }
    else
    {
        Console.WriteLine(ready.Message);
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
