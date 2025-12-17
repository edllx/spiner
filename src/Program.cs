//using Spectre.Console;

using spinner;

using App app = new(string.Join(" ", args));
var podman = new PodmanService();

try
{
    app.Init();
    await app.Start();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
