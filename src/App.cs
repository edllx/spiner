using System.Text;
using System.Threading.Tasks;

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

    private readonly TaskBatch _imageBuildTasks;
    private readonly TaskBatch _cleanUpTasks;
    private readonly TaskBatch _podTasks;
    private readonly TaskBatch _testsBatch;

    private List<BaseTask> TestTasks = [];

    private readonly Dictionary<string, int> _portMapping = [];

    public App(string args)
    {
        Args = args;
        _imageBuildTasks = new();
        _cleanUpTasks = new();
        _podTasks = new();
        _testsBatch = new();
        _cleanUpTasks.OnTaskFinished += HandleTaskCleaned;
        _imageBuildTasks.OnTaskFinished += HandleImageBuilt;
        _podTasks.OnTaskFinished += HandlePodBuilt;
        _testsBatch.OnTaskFinished += HandleTestsFinished;
    }

    private void HandleTestsFinished(object? sender, TaskResultBase e)
    {
        _ = CleanUp();
    }

    private void HandlePodBuilt(object? sender, TaskResultBase e)
    {
        CreateTests();
        _ = _taskManager.ScheduleTask(_testsBatch);
    }

    private void HandleTaskCleaned(object? sender, TaskResultBase e)
    {
        _done = true;
    }

    private void HandleImageBuilt(object? sender, TaskResultBase e)
    {
        // Do somthing
        // Ready to build test
        _ = CreatePods();
    }

    public App()
    {
        _imageBuildTasks = new();
        _cleanUpTasks = new();
        _podTasks = new();
        _testsBatch = new();
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

            _cleanUpTasks.Add(async () =>
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

        //_imageBuildTasks.SetTasks(imagesTask);
        await _taskManager.ScheduleTask(_imageBuildTasks);
        _taskManager.Start();
    }

    private async Task CreatePods()
    {
        foreach (TestSuite suite in TestManager.Tests)
        {
            foreach (Tests tests in suite.TestSet)
            {
                TaskBatch batch = new();
                var podName = Tools.GenerateRandomString(32, "pod-");
                int port = Random.Shared.Next(3500, 6500);

                while (_portMapping.ContainsValue(port))
                {
                    port = Random.Shared.Next(3500, 6500);
                }

                _portMapping.Add(tests.Id, port);

                foreach (var item in suite.TestStack.Services)
                {
                    batch.Add(async () =>
                    {
                        try
                        {
                            Console.WriteLine($"Creating Container: {item.Name} in Pod: {podName}");
                            await _podman.RunContainerAsync(
                                item.Image,
                                $"sp-{item.Name}",
                                ports: [],
                                pod: podName,
                                envVariables: item.Scope.Keys.Select(v =>
                                    {
                                        return (v.Name, v.Value);
                                    })
                                    .ToArray()
                            );
                            await _podman.ExecCommandAsync($"sp-{item.Name}", "rm -rf scripts");
                            await _podman.ExecCommandAsync($"sp-{item.Name}", "mkdir scripts");

                            foreach (var cmd in item.Commands)
                            {
                                switch (cmd)
                                {
                                    case Copy cp:

                                        var filename = cp.Source.Split("/").Last().ToString();
                                        await _podman.Copy(
                                            cp.Source,
                                            $"{cp.Destination}/{filename}",
                                            $"sp-{item.Name}"
                                        );
                                        break;

                                    case Run run:
                                        Console.WriteLine(run.Text);
                                        await _podman.ExecCommandAsync($"sp-{item.Name}", run.Text);

                                        break;
                                }
                            }
                            return new();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            return new() { Success = false, Error = ex.Message };
                        }
                    });
                }

                _podTasks.Add(async () =>
                {
                    try
                    {
                        Console.WriteLine($"Creating Pod: {podName}");
                        await _podman.BuildPod(podName, [(port, 8080)]);
                        await batch.Run();
                        return new();
                    }
                    catch (Exception ex)
                    {
                        return new() { Success = false, Error = ex.Message };
                    }
                });

                _cleanUpTasks.Add(async () =>
                {
                    await _podman.RemovePod(podName);
                    return new();
                });
            }
        }

        await _taskManager.ScheduleTask(_podTasks);
    }

    private void CreateTests()
    {
        foreach (TestSuite suite in TestManager.Tests)
        {
            foreach (Tests tests in suite.TestSet)
            {
                AddTestsToBatch(tests);
            }
        }
    }

    private void AddTestsToBatch(Tests tests)
    {
        _portMapping.TryGetValue(tests.Id, out int port);
        if (port == 0)
        {
            return;
        }
        switch (tests.Mode)
        {
            case "sync":
                TaskSequence seq = new();
                foreach (Test test in tests.TestSet)
                {
                    var f = HandleRequest(port, test);
                    seq.Add(f);
                }

                _testsBatch.Add(async () =>
                {
                    var res = await seq.Run();
                    return new();
                });
                break;

            default:

                foreach (var test in tests.TestSet)
                {
                    var f = HandleRequest(port, test);
                    _testsBatch.Add(f);
                }
                break;
        }
    }

    private static Func<Task<TaskResult>> HandleRequest(int port, Test test)
    {
        return async () =>
        {
            using var contex = new HttpContext(new() { BaseUri = $"http://localhost:{port}" });

            var method = test.Request?.Method ?? "GET";
            var path = test.Request?.Path ?? "";
            var body = test.Request?.Body?.Model();

            HttpResponse? response = null;
            try
            {
                Console.WriteLine($"{method} Request to: http://localhost:{port}/{path}");
                switch (method)
                {
                    case "POST":
                        response = await contex.Post(path, body);
                        break;

                    case "PATCH":
                        response = await contex.Patch(path, body);
                        break;

                    case "PUT":
                        response = await contex.Put(path, body);
                        break;

                    // GET
                    default:
                        response = await contex.Get(path);
                        break;
                }

                if (response is not null)
                {
                    foreach (var item in test.Asserts?.Asserts ?? [])
                    {
                        switch (item)
                        {
                            case AssertEquals eq:
                                eq.Exptected = response.JsonFind(eq.Exptected, test.Scope).Value;
                                eq.Actual = response.JsonFind(eq.Actual, test.Scope).Value;
                                eq.evaluate();
                                new AssertEquals(
                                    response.JsonFind(eq.Exptected, test.Scope).Value,
                                    response.JsonFind(eq.Actual, test.Scope).Value
                                );
                                break;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (response is not null)
                {
                    response.Dispose();
                }
            }
            return new();
        };
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

        if (_cleanUpTasks is not null)
        {
            Console.WriteLine("Cleanning Tasks");
            await _cleanUpTasks.Run();
        }

        await _podman.PruneImages();
        Console.WriteLine("CleanUp done");
    }

    public void Dispose()
    {
        if (_imageBuildTasks is not null)
        {
            _imageBuildTasks.OnTaskFinished -= HandleImageBuilt;
        }

        _podTasks.OnTaskFinished -= HandlePodBuilt;
        _cleanUpTasks.OnTaskFinished -= HandleTaskCleaned;
        _testsBatch.OnTaskFinished -= HandleTestsFinished;
    }
}
