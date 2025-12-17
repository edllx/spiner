//using Spectre.Console;

using spinner;

using App app = new(string.Join(" ", args));
var podman = new PodmanService();

try
{
    bool ready = app.Init();

    if (ready)
    {
        await app.Start();
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
