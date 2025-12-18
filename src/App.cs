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

public class CLICommandOutput
{
    public CLICommandOutput(bool success, Exception? exception, string message)
    {
        Success = success;
        Exception = exception;
        Message = message;
    }

    public bool Success { get; init; }
    public Exception? Exception { get; init; }
    public string Message { get; init; } = "";
}

public partial class App : IDisposable
{
    public string Args { get; } = "";
    private CLIArgParser Parser = new();
    private string _inputFile = "";
    private List<(string, string)> Options = [];
    private List<string> Path = [];

    public bool Done { get; private set; } = false;
    public bool Debug { get; private set; } = false;
    public bool ImageRebuild { get; private set; } = true;

    private readonly TaskManager _taskManager = new();
    private readonly PodmanService _podman = new();
    public readonly Logger Logger = new();

    public ServiceManager ServiceManager { get; set; } = new();
    public RequestManager RequestsManager { get; set; } = new();
    public TestsManager TestManager { get; set; } = new();

    private readonly TaskBatch _imageBuildTasks;
    private readonly TaskBatch _cleanUpTasks;
    private readonly TaskBatch _podTasks;
    private readonly TaskBatch _testsBatch;
    private readonly TaskBatch _testsLogs;

    public readonly Dictionary<string, int> PortMapping = [];

    public App(string args)
    {
        Args = args;
        _imageBuildTasks = new();
        _cleanUpTasks = new();
        _podTasks = new();
        _testsBatch = new();
        _testsLogs = new();
        _cleanUpTasks.OnTaskFinished += HandleTaskCleaned;
        _imageBuildTasks.OnTaskFinished += HandleImageBuilt;
        _podTasks.OnTaskFinished += HandlePodBuilt;
        _testsBatch.OnTaskFinished += HandleTestsFinished;
        _testsLogs.OnTaskFinished += HandleTestsLogsFinished;
    }

    private void HandleTestsLogsFinished(object? sender, TaskResultBase e)
    {
        _ = CleanUp();
    }

    private void HandleTestsFinished(object? sender, TaskResultBase e)
    {
        _ = _taskManager.ScheduleTask(_testsLogs);
    }

    private void HandlePodBuilt(object? sender, TaskResultBase e)
    {
        CreateTests();
        _ = _taskManager.ScheduleTask(_testsBatch);
    }

    private void HandleTaskCleaned(object? sender, TaskResultBase e)
    {
        Done = true;
    }

    private void HandleImageBuilt(object? sender, TaskResultBase e)
    {
        _ = CreatePods();
    }

    public App()
    {
        _testsLogs = new();
        _imageBuildTasks = new();
        _cleanUpTasks = new();
        _podTasks = new();
        _testsBatch = new();
    }

    public CLICommandOutput Init()
    {
        try
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
        }
        catch (MissingCommandArgument ex)
        {
            return new(
                false,
                ex,
                $"Missing input file\n\n{CLIArgParser.Help(string.Join(" ", Path))}"
            );
        }
        catch (UnknownCommandExeption ex)
        {
            return new(false, ex, CLIArgParser.Help(string.Join(" ", Path)));
        }
        catch (System.Exception ex)
        {
            return new(false, ex, CLIArgParser.Help(string.Join(" ", Path)));
        }

        Execute();
        return new(true, null, "");
    }

    public async Task Start()
    {
        _ = BuildImages();
        await Loop();
    }

    private async Task Loop()
    {
        while (!Done)
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

            switch (key)
            {
                case "-f":
                case "--file":
                    SetFileName(value);
                    break;

                case "-h":
                case "--help":
                    Console.WriteLine(CLIArgParser.Help(string.Join(" ", Path)));
                    throw new Exception("show help");

                case "--debug":
                    Debug = true;
                    break;

                case "--no-image-rebuild":
                    ImageRebuild = false;
                    break;
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
            Logger.Log("Input file: {_inputFile} not found", logLevel: LogLevel.Critial);
            return;
        }
        string source = File.ReadAllText(_inputFile);
        var res = parser.Parse(source);

        if (!res.Success || res.Token is not SpinnerToken token)
        {
            Logger.Log(
                $"Input file parsing failed: {res.ToString(_inputFile)}",
                logLevel: LogLevel.Error
            );
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

            _imageBuildTasks.Add(async () =>
            {
                try
                {
                    bool imageExist = await _podman.ImageExist(template.ImageName);
                    if (imageExist && !ImageRebuild)
                    {
                        if (Debug)
                        {
                            Logger.Log(
                                $"Image {template.ImageName} exist and image-rebuid disabled",
                                logLevel: LogLevel.Debug
                            );
                        }

                        return new();
                    }

                    if (imageExist)
                    {
                        Logger.Log(
                            $"Rebuilding Image {template.ImageName}",
                            logLevel: LogLevel.Info
                        );
                    }
                    else
                    {
                        Logger.Log($"Building Image {template.ImageName}", logLevel: LogLevel.Info);
                    }

                    await _podman.BuildImageAsync(
                        buildFilePath: template.BuildPath,
                        context: ctx,
                        tag: template.ImageName
                    );

                    return new();
                }
                catch (Exception ex)
                {
                    Logger.Log(
                        $"Image {template.ImageName} Creation failed\n{ex.Message}",
                        logLevel: LogLevel.Warning
                    );

                    return new() { Success = false, Error = ex.Message };
                }
            });
        }
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

                while (PortMapping.ContainsValue(port))
                {
                    port = Random.Shared.Next(3500, 6500);
                }

                PortMapping.Add(tests.Id, port);

                foreach (Service item in suite.TestStack.Services)
                {
                    var containerName = Tools.GenerateRandomString(
                        item.Name.Length + 16,
                        $"sp-{item.Name}-"
                    );

                    if (Debug && item.LogEnabled)
                    {
                        _testsLogs.Add(async () =>
                        {
                            string logs = await _podman.GetContainerLogs(containerName);
                            Logger.Log($"Logs:{containerName}\n{logs}", LogLevel.Debug);
                            return new();
                        });
                    }

                    batch.Add(async () =>
                    {
                        try
                        {
                            Logger.Log($"Creating Container: {containerName} in Pod: {podName}");
                            await _podman.RunContainerAsync(
                                item.Image,
                                containerName,
                                ports: [],
                                pod: podName,
                                envVariables: item.Scope.Keys.Select(v =>
                                    {
                                        return (v.Name, v.Value);
                                    })
                                    .ToArray()
                            );
                            await _podman.ExecCommandAsync(containerName, "rm -rf scripts");
                            await _podman.ExecCommandAsync(containerName, "mkdir scripts");

                            var cmdSequence = new TaskSequence();

                            for (int i = 0; i < item.Commands.Length; i++)
                            {
                                var cmd = item.Commands[i];
                                try
                                {
                                    string command = cmd.ToString(0);
                                    if (command.Length > 40)
                                    {
                                        command = $"{command.AsSpan().Slice(0, 40).ToString()} ...";
                                    }

                                    switch (cmd)
                                    {
                                        case Copy cp:

                                            var filename = cp.Source.Split("/").Last().ToString();
                                            cmdSequence.Add(async () =>
                                            {
                                                if (Debug)
                                                {
                                                    Logger.Log(
                                                        $"Copy :{filename} in : {containerName}",
                                                        LogLevel.Debug
                                                    );
                                                }

                                                await _podman.Copy(
                                                    cp.Source,
                                                    $"{cp.Destination}/{filename}",
                                                    containerName
                                                );

                                                return new();
                                            });
                                            break;

                                        case Run run:
                                            cmdSequence.Add(async () =>
                                            {
                                                if (Debug)
                                                {
                                                    Logger.Log(
                                                        $"Run :{run.Text} in : {containerName}",
                                                        LogLevel.Debug
                                                    );
                                                }

                                                await _podman.ExecCommandAsync(
                                                    containerName,
                                                    run.Text
                                                );
                                                return new();
                                            });
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log(
                                        $"Failed command on container: {containerName} : {ex.Message}",
                                        LogLevel.Warning
                                    );
                                }
                            }

                            await cmdSequence.Run();

                            Logger.Log($"Container: {containerName} Created");

                            return new();
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Comand failed: {ex.Message}", LogLevel.Error);
                            return new() { Success = false, Error = ex.Message };
                        }
                    });
                }

                _podTasks.Add(async () =>
                {
                    try
                    {
                        Logger.Log($"Creating Pod: {podName}");
                        await _podman.BuildPod(podName, [(port, 8080)]);
                        await batch.Run();
                        return new();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Comand failed: {ex.Message}", LogLevel.Error);
                        return new() { Success = false, Error = ex.Message };
                    }
                });

                _cleanUpTasks.Add(async () =>
                {
                    Logger.Log($"Cleanning up Pod: {podName}");

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
        PortMapping.TryGetValue(tests.Id, out int port);
        if (port == 0)
        {
            return;
        }

        switch (tests.Mode)
        {
            case "sync":
                TaskSequence seq = new();
                for (int i = 0; i < tests.TestSet.Length; i++)
                {
                    var f = HandleRequest(port, tests.TestSet[i], this);
                    seq.Add(f);
                }

                _testsBatch.Add(async () =>
                {
                    var res = await seq.Run();

                    return new();
                });
                break;

            default:
                for (int i = 0; i < tests.TestSet.Length; i++)
                {
                    var f = HandleRequest(port, tests.TestSet[i], this);

                    _testsBatch.Add(f);
                }

                break;
        }
    }

    private static Func<Task<TaskResult>> HandleRequest(int port, Test test, App app)
    {
        return async () =>
        {
            using var contex = new HttpContext(new() { BaseUri = $"http://localhost:{port}" });

            if (test.Request is not null && test.Request.Body is not null)
            {
                test.Request.Body.Resolve(test.Scope);
            }

            if (test.Request is not null && test.Asserts is not null)
            {
                test.Asserts.Resolve(test.Scope);
            }

            var method = test.Request?.Method ?? "GET";
            var path = test.Request?.Path ?? "";
            var body = test.Request?.Body?.Model();
            var id = Tools.GenerateRandomString(12, "Test-");

            var resolvedPath = KeyManager.Resolve(path, test.Scope);

            HttpResponse? response = null;
            try
            {
                app.Logger.Log($"{id}:{method}: localhost:{port}/{resolvedPath}");
                switch (method)
                {
                    case "POST":
                        if (app.Debug && test.Request is not null && test.Request.Body is not null)
                        {
                            app.Logger.Log($"\n{test.Request.Body.ToString(0)}", LogLevel.Debug);
                        }

                        response = await contex.Post(resolvedPath, body);
                        break;

                    case "PATCH":
                        if (app.Debug && test.Request is not null && test.Request.Body is not null)
                        {
                            app.Logger.Log($"\n{test.Request.Body.ToString(0)}", LogLevel.Debug);
                        }
                        response = await contex.Patch(resolvedPath, body);
                        break;

                    case "PUT":
                        if (app.Debug && test.Request is not null && test.Request.Body is not null)
                        {
                            app.Logger.Log($"\n{test.Request.Body.ToString(0)}", LogLevel.Debug);
                        }
                        response = await contex.Put(resolvedPath, body);
                        break;

                    // GET
                    default:
                        response = await contex.Get(resolvedPath);
                        break;
                }

                if (response is not null)
                {
                    //Console.WriteLine(response.ToString());
                    foreach (var item in test.Response?.Setters ?? [])
                    {
                        var valueR = response.JsonFind(item.Value, test.Scope);
                        if (!valueR.Found)
                        {
                            app.Logger.Log(
                                $"{id}:Set: Did not found a value for {item.Value}",
                                LogLevel.Warning
                            );
                        }
                        else
                        {
                            test.Scope.Set(item.Key, valueR.Value);
                        }
                    }

                    foreach (var item in test.Asserts?.Asserts ?? [])
                    {
                        bool result = false;
                        switch (item)
                        {
                            case AssertEquals eq:
                                var eqq = new AssertEquals(
                                    response.JsonFind(eq.Exptected, test.Scope).Value,
                                    response.JsonFind(eq.Actual, test.Scope).Value
                                );
                                result = eqq.evaluate().Success;

                                app.Logger.Log(
                                    $"{id}:Assert: {eq.Actual} == {eq.Exptected} {(result ? $"{AnsiColors.Colorize("Success", AnsiColors.Green)}" : $"{AnsiColors.Colorize("Failed", AnsiColors.Red)} Found: {eqq.Actual}")}"
                                );

                                break;
                            case AssertNotNull ntn:
                                var ntnValue = test.Scope.Get(ntn.Key);
                                var isEmpty = string.IsNullOrEmpty(ntnValue);

                                app.Logger.Log(
                                    $"{id}:Assert: {ntn.Key} NOT NULL {(!isEmpty ? $"{AnsiColors.Colorize("Success", AnsiColors.Green)} Found: {ntnValue}" : $"{AnsiColors.Colorize("Failed", AnsiColors.Red)}")}"
                                );

                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                app.Logger.Log($"Something failed: {ex.Message}");
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
        Logger.Log($"Cleanup started", logLevel: LogLevel.Info);

        if (_cleanUpTasks is not null)
        {
            Logger.Log($"Cleanning tasks", logLevel: LogLevel.Info);
            await _cleanUpTasks.Run();
        }

        await _podman.PruneImages();
        Logger.Log($"Cleanup done", logLevel: LogLevel.Info);
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
