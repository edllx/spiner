//using Spectre.Console;

using spinner;

using App app = new(string.Join(" ", args));

try
{
    app.Init();
    await app.Start();
    //Console.WriteLine(app.ToString(0));
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
