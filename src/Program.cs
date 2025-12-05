//using Spectre.Console;

using spinner;

App app = new(string.Join(" ", args));

try
{
    app.Init();

    Console.WriteLine(app.ToString());
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
