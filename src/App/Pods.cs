using System.Text;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace spinner;

public enum AssertResutState
{
    Skipped,
    Success,
    Failed,
}

public abstract class TestResult
{
    public string Name;

    public TestResult(string name)
    {
        Name = name;
    }

    public virtual Spectre.Console.Rendering.IRenderable Format()
    {
        return new Markup(Name);
    }
}

public class TestResultTree : TestResult
{
    public List<TestResult> Branches { get; init; } = [];

    public TestResultTree(string name, List<TestResult> branches)
        : base(name)
    {
        Branches = branches;
    }

    public override IRenderable Format()
    {
        Tree res = new(Name);

        for (int i = 0; i < Branches.Count; i++)
        {
            res.AddNode(Branches[i].Format());
        }

        return res;
    }
}

public class TestResultLeaf : TestResult
{
    public bool Success { get; init; } = false;
    public string Message { get; init; }

    public TestResultLeaf(string name, bool success, string message = "")
        : base(name)
    {
        Success = success;
        Message = message;
    }

    public override IRenderable Format()
    {
        StringBuilder builder = new();

        if (Success)
        {
            builder.Append($"{Emoji.Known.CheckMarkButton} {Name}");
            return new Markup(builder.ToString());
        }
        else
        {
            builder.Append($"{Emoji.Known.CrossMark} {Name}");

            if (!string.IsNullOrEmpty(Message))
            {
                builder.Append($"\n{Message}");
            }

            return new Markup($"{builder.ToString()}");
        }
    }
}

public partial class App
{
    private TestResult AddTestPods(TaskBatch podBatch)
    {
        List<TestResult> results = [];
        TestResultTree root = new("Tests", branches: results);
        int i = 1;

        foreach (TestSuite suite in TestManager.Tests)
        {
            List<TestResult> suites = [];
            TestResultTree testSuite = new($"TestSuit-{i++}", branches: suites);
            results.Add(testSuite);

            foreach (Tests tests in suite.TestSet)
            {
                TaskSequence testsSequence = new();
                TaskBatch serviceBatch = new();
                var podName = Tools.GenerateRandomString(32, "pod-");
                int port = FindAvailablePort();
                PortMapping.Add(tests.Id, port);
                List<string> serviceTolog = [];

                foreach (Service service in suite.TestStack.Services)
                {
                    var serviceName = AddPodServiceTask(serviceBatch, service, podName, port);
                    if (service.LogEnabled)
                    {
                        serviceTolog.Add(serviceName);
                    }
                }

                testsSequence.Add(async () =>
                {
                    try
                    {
                        await _podman.BuildPod(podName, [(port, 8080)]);
                        await serviceBatch.Run();
                        Logger.Log($"Pod {podName} Ready", LogLevel.Info);

                        BaseTask testRuns = CreateTests(tests, port, testSuite);
                        await testRuns.Run();
                        foreach (string sName in serviceTolog)
                        {
                            string logs = await _podman.GetContainerLogs(sName);
                            Logger.Log($"Logs:{sName}\n{logs}", LogLevel.Debug);
                        }

                        await _podman.RemovePod(podName);
                        return new();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Comand failed: {ex.Message}", LogLevel.Warning);
                        return new() { Success = false, Error = ex.Message };
                    }
                });

                podBatch.Add(async () =>
                {
                    await testsSequence.Run();
                    return new();
                });
            }
        }
        return root;
    }

    private void AddPodCleanupTask(BaseTask sequence, string podName)
    {
        sequence.Add(async () =>
        {
            Logger.Log($"Cleanning up Pod: {podName}");
            await _podman.RemovePod(podName);
            return new();
        });
    }

    private string AddPodServiceTask(TaskBatch sequence, Service service, string podName, int port)
    {
        var containerName = Tools.GenerateRandomString(
            service.Name.Length + 16,
            $"sp-{service.Name}-"
        );

        sequence.Add(async () =>
        {
            try
            {
                await _podman.RunContainerAsync(
                    service.Image,
                    containerName,
                    ports: [],
                    pod: podName,
                    envVariables: service
                        .Scope.Keys.Select(v =>
                        {
                            return (v.Name, v.Value);
                        })
                        .ToArray()
                );
                await _podman.ExecCommandAsync(containerName, "rm -rf scripts");
                await _podman.ExecCommandAsync(containerName, "mkdir scripts");

                var cmdSequence = new TaskSequence();

                for (int i = 0; i < service.Commands.Length; i++)
                {
                    var cmd = service.Commands[i];
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

                                    await _podman.ExecCommandAsync(containerName, run.Text);
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
        return containerName;
    }

    private int FindAvailablePort()
    {
        int port = Random.Shared.Next(3500, 6500);

        while (PortMapping.ContainsValue(port))
        {
            port = Random.Shared.Next(3500, 6500);
        }
        return port;
    }
}
