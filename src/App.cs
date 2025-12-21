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

    public bool Done { get; set; } = false;
    public bool Debug { get; private set; } = false;
    public bool ImageRebuild { get; private set; } = true;

    private readonly TaskManager _taskManager = new();
    private readonly PodmanService _podman = new();
    public readonly Logger Logger = new();

    public ServiceManager ServiceManager { get; set; } = new();
    public RequestManager RequestsManager { get; set; } = new();
    public TestsManager TestManager { get; set; } = new();

    public event Action<TaskResultBase>? OnTaskDone;
    public readonly Dictionary<string, int> PortMapping = [];
    public TestResult? Results { get; private set; }

    public App(string args)
    {
        Args = args;
        _taskManager.Start();
    }

    public App()
    {
        _taskManager.Start();
    }

    public async Task Start()
    {
        _ = BuildImages();
    }

    public async Task BuildImages()
    {
        TaskSequence imageBuilSequence = new() { Tag = "ImageBuild" };
        imageBuilSequence.OnTaskDone += NotifyTaskDone;
        AddImageTasks(imageBuilSequence);

        await _taskManager.ScheduleTask(imageBuilSequence);
    }

    public async Task RunTests()
    {
        TaskBatch testRuns = new() { Tag = "TestRuns" };
        testRuns.OnTaskDone += NotifyTaskDone;
        Results = AddTestPods(testRuns);

        await _taskManager.ScheduleTask(testRuns);
    }

    private void NotifyTaskDone(TaskResultBase result)
    {
        OnTaskDone?.Invoke(result);
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

    public void Dispose()
    {
        _taskManager.Dispose();
    }
}
