using Spectre.Console;

namespace spinner;

public partial class App
{
    private void AddTestPods(TaskBatch podBatch)
    {
        foreach (TestSuite suite in TestManager.Tests)
        {
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

                        BaseTask testRuns = CreateTests(tests, port);
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

        /*
        if (Debug && service.LogEnabled)
        {
            _testsLogs.Add(async () =>
            {
                string logs = await _podman.GetContainerLogs(containerName);
                Logger.Log($"Logs:{containerName}\n{logs}", LogLevel.Debug);
                return new();
            });
        }
        */

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
