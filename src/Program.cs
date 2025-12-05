//using Spectre.Console;

using spinner;

App app = new(string.Join(" ", args));

try
{
    app.Init();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
