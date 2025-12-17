//using Spectre.Console;

using spinner;

using App app = new(string.Join(" ", args));

try
{
    app.Init();
    await app.Start();
    //Console.WriteLine(KeyManager.Resolve("{{name}}", new Scope([new("name", "ali")])));
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
