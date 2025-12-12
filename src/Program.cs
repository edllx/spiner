//using Spectre.Console;

using spinner;

using App app = new(string.Join(" ", args));

try
{
    app.Init();
    await app.Start();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
