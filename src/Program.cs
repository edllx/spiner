//using Spectre.Console;

using spinner;

SpinnerParser parser = new();

App app = new(string.Join(" ", args));

try
{
    app.Init();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
