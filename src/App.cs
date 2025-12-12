using System.Text;

namespace spinner;

public abstract class HandleElementRequest<T>
{
    public HandleElementRequest(IToken token, string source)
    {
        Token = token;
        this.Source = source;
    }

    public IToken Token { get; init; }
    public string Source { get; init; }
}

public partial class App : IDisposable
{
    public string Args { get; } = "";
    private CLIArgParser Parser = new();
    private string _inputFile = "";
    private List<(string, string)> Options = [];
    private List<string> Path = [];
    public string ErrorMessage = "";
    bool _done = false;

    private readonly TaskManager _taskManager = new();
    private readonly PodmanService _podman = new();

    public ServiceManager ServiceManager { get; set; } = new();
    public RequestManager RequestsManager { get; set; } = new();
    public TestsManager TestManager { get; set; } = new();

    private readonly TaskBatch ImageBuildTasks;
    private readonly TaskBatch CleanUpTasks;

    public App(string args)
    {
        Args = args;
        ImageBuildTasks = new();
        CleanUpTasks = new();
        CleanUpTasks.OnTaskFinished += HandleTaskCleaned;
        ImageBuildTasks.OnTaskFinished += HandleImageBuilt;
    }

    private void HandleTaskCleaned(object? sender, TaskResultBase e)
    {
        _done = true;
    }

    private void HandleImageBuilt(object? sender, TaskResultBase e)
    {
        // Do somthing
        // Ready to build test
        _ = CleanUp();
    }

    public App()
    {
        ImageBuildTasks = new();
        CleanUpTasks = new();
    }

    public void Init()
    {
        ParseResult res = Parser.Parse(new ParseContext(Args));

        if (!res.Success)
        {
            throw new Exception("Fail to parse args");
        }

        CommandToken token = (CommandToken)res.Token;

        UnwrapCommand(token);

        if (string.IsNullOrEmpty(_inputFile))
        {
            throw new MissingCommandArgument("input file");
        }
        Execute();
    }

    public async Task Start()
    {
        _ = BuildImages();
        await Loop();
    }

    private async Task Loop()
    {
        while (!_done)
        {
            await Task.Delay(2000);
        }
    }

    private void UnwrapCommand(CommandToken token)
    {
        Path.Add(token.Name.ToString(Args));
        if (token.Arg.IsSet())
        {
            SetFileName(token.Arg.ToString(Args));
        }

        for (int i = 0; i < token.Options.Length; i++)
        {
            var el = token.Options[i];
            var key = el.Key.ToString(Args);
            var value = el.Value.ToString(Args);
            if (el.Required && string.IsNullOrEmpty(value))
            {
                throw new MissingOptionArgument(key);
            }

            if (key == "-f" || key == "--file")
            {
                SetFileName(value);
            }

            Options.Add((key, value));
        }

        if (token.Child is null)
        {
            return;
        }
        UnwrapCommand(token.Child);
    }

    private void SetFileName(string name)
    {
        if (!string.IsNullOrEmpty(_inputFile))
        {
            return;
        }
        _inputFile = name;
    }

    private void Execute()
    {
        switch (string.Join(" ", Path))
        {
            case "run":
                Build();
                break;
            default:

                break;
        }
    }

    private void Build()
    {
        SpinnerParser parser = new SpinnerParser();
        if (!File.Exists(_inputFile))
        {
            return;
        }
        string source = File.ReadAllText(_inputFile);
        var res = parser.Parse(source);

        if (!res.Success || res.Token is not SpinnerToken token)
        {
            Console.WriteLine(res.ToString(source));
            return;
        }

        List<TestSuite> testSuites = [];

        for (int i = 0; i < token.Children.Length; i++)
        {
            if (token.Children[i] is not SpinnerToken tk)
            {
                continue;
            }

            switch (tk.Name)
            {
                case "Services":
                    ServiceManager.SetTemplates(
                        HandleElement<List<ServiceTemplate>>(new(tk, source))
                    );
                    break;

                case "Requests":
                    RequestsManager.SetTemplates(
                        HandleElement<List<RequestTemplate>>(new(tk, source))
                    );
                    break;
            }
        }

        for (int i = 0; i < token.Children.Length; i++)
        {
            if (token.Children[i] is not SpinnerToken tk)
            {
                continue;
            }

            switch (tk.Name)
            {
                case "TestSuite":

                    var ts = HandleElement<TestSuite>(new(tk, source));
                    if (ts is not null)
                    {
                        testSuites.Add(ts);
                    }
                    break;
            }
        }

        TestManager.SetTemplates(testSuites);
    }

    private async Task BuildImages()
    {
        List<Func<Task<TaskResult>>> imagesTask = [];
        for (int i = 0; i < ServiceManager.Templates.Count; i++)
        {
            var template = ServiceManager.Templates[i];
            if (string.IsNullOrEmpty(template.BuildPath))
            {
                continue;
            }
            var parts = template.BuildPath.Split("/");
            StringBuilder b = new();

            for (int j = 1; j < parts.Length - 1; j++)
            {
                b.Append($"/{parts[j]}");
            }
            var ctx = b.ToString();
            imagesTask.Add(async () =>
            {
                try
                {
                    Console.WriteLine($"Creating Image {template.ImageName}");

                    await _podman.BuildImageAsync(
                        buildFilePath: template.BuildPath,
                        context: ctx,
                        tag: template.ImageName
                    );

                    return new();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Image {template.ImageName} Creation failed\n{ex.Message}");
                    return new() { Success = false, Error = ex.Message };
                }
            });

            CleanUpTasks.Add(async () =>
            {
                try
                {
                    await _podman.RemoveImageAsync(template.ImageName);
                    return new();
                }
                catch (Exception ex)
                {
                    return new() { Success = false, Error = ex.Message };
                }
            });
        }

        ImageBuildTasks.SetTasks(imagesTask);
        await _taskManager.ScheduleTask(ImageBuildTasks);
        _taskManager.Start();
    }

    public override string ToString()
    {
        return "";
    }

    public string ToString(int depth)
    {
        return string.Join(
            "\n\n",
            [
                ServiceManager.ToString(depth),
                RequestsManager.ToString(depth),
                TestManager.ToString(depth),
            ]
        );
    }

    private async Task CleanUp()
    {
        Console.WriteLine("Cleaning");

        if (CleanUpTasks is not null)
        {
            await CleanUpTasks.Run();
        }

        await _podman.PruneImages();
        Console.WriteLine("CleanUp done");
    }

    public void Dispose()
    {
        if (ImageBuildTasks is not null)
        {
            ImageBuildTasks.OnTaskFinished -= HandleImageBuilt;
        }
    }
}
