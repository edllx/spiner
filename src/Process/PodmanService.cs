using System.Diagnostics;
using System.Text;

namespace spinner;

public enum ContainerStatus
{
    // Basic lifecycle states
    created,
    running,
    exited,
    paused,
    restarting,

    // Removal/cleanup states
    removing,
    dead,

    // Podman-specific states
    configured,
    initialized,

    // Health check states (when healthchecks are configured)
    starting,
    healthy,
    unhealthy,

    // Unknown/error state
    unknown,
}

public partial class PodmanService : IContainerService
{
    public async Task BuildImageAsync(
        string buildFilePath,
        string context,
        string tag,
        CancellationToken? token = null
    )
    {
        CancellationToken tk = token ?? new();
        var command = $"build --force-rm -t {tag} -f {buildFilePath} {context}";

        var result = await Run(command, tk);

        switch (result.ExitCode)
        {
            case 0:
                break;
            default:
                Console.WriteLine(result.StdErr);
                throw new Exception(result.StdErr);
        }
    }

    public async Task RemoveImageAsync(string tag, CancellationToken? token = null)
    {
        CancellationToken tk = token ?? new();
        var command = $"image rm {tag}";

        var result = await Run(command, tk);

        switch (result.ExitCode)
        {
            case 0:
                break;
            default:
                Console.WriteLine(result.StdErr);
                throw new Exception(result.StdErr);
        }
    }

    public async Task PruneImages()
    {
        var command = $"image prune";

        CancellationToken tk = new();
        var result = await Run(command, tk, prompt: "y");
    }

    private async Task ExecCommandAsyncCommit(
        string containerId,
        string cmd,
        CancellationToken? token
    )
    {
        CancellationToken tk = token ?? new();
        var command = $"exec {containerId} {cmd}";
        try
        {
            var result = await Run(command, tk);
            switch (result.ExitCode)
            {
                case 0:
                    Console.WriteLine(result.StdOut);
                    break;
                default:
                    Console.WriteLine(result.StdErr);
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task ExecCommandAsync(
        string containerId,
        string cmd,
        CancellationToken? token = null
    )
    {
        var state = await GetContainerState(containerId);
        switch (state)
        {
            case ContainerStatus.running:
                await ExecCommandAsyncCommit(containerId, cmd, token);
                break;
            default:
                await Task.Delay(200);
                await ExecCommandAsync(containerId, cmd, token);
                break;
        }
    }

    public async Task IsContainerHealthyAsync(string containerId)
    {
        var state = await GetContainerState(containerId);
    }

    private async Task<ContainerStatus> GetContainerState(string containerId)
    {
        var format = "{{.State.Status}}";
        var command = $"inspect {containerId} --format \"{format}\"";
        CancellationToken token = new();
        ProcessResult result = await Run(command, token);

        var stringStatus = result.StdOut.Trim([' ', '\n']);
        try
        {
            return Enum.Parse<ContainerStatus>(stringStatus);
        }
        catch (System.Exception)
        {
            return ContainerStatus.unknown;
        }
    }

    public async Task RemoveContainerAsync(string containerId, bool force = false)
    {
        var command = new StringBuilder();
        command.Append($"rm {containerId}");
        if (force)
        {
            command.Append(" -f");
        }
        CancellationToken token = new();
        await Run(command.ToString(), token);
    }

    public async Task RunContainerAsync(string image, string name, bool replace = true)
    {
        await RunContainerAsync(image, name, [], [], replace: replace);
    }

    public async Task RunContainerAsync(
        string image,
        string name,
        (int, int)[] ports,
        bool replace = true
    )
    {
        await RunContainerAsync(image, name, ports: ports, envVariables: [], replace: replace);
    }

    public async Task RunContainerAsync(
        string image,
        string name,
        (string, string)[] envVariables,
        (int, int)[] ports,
        bool replace = true
    )
    {
        var command = new StringBuilder();

        command.Append("run -d");
        command.Append($" --name {name}");

        if (replace)
        {
            command.Append(" --replace");
        }

        for (int i = 0; i < envVariables.Length; i++)
        {
            command.Append($" -e {envVariables[i].Item1}={envVariables[i].Item2}");
        }

        for (int i = 0; i < ports.Length; i++)
        {
            command.Append($" -p {ports[i].Item1}:{ports[i].Item2}");
        }

        command.Append($" {image}");

        try
        {
            var result = await Run(command.ToString(), new());
            switch (result.ExitCode)
            {
                case 0:
                    Console.WriteLine(result.StdOut);
                    break;
                default:
                    Console.WriteLine(result.StdErr);
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task StopContainerAsync(string containerId)
    {
        await Task.CompletedTask;
    }
}

public struct ProcessResult
{
    public int ExitCode;
    public string StdOut;
    public string StdErr;

    public ProcessResult(int exitCode, string stdOut, string stdErr)
    {
        ExitCode = exitCode;
        StdOut = stdOut;
        StdErr = stdErr;
    }
}

public partial class PodmanService
{
    private async Task<ProcessResult> Run(
        string command,
        CancellationToken token,
        string workingDirectory = "",
        string prompt = ""
    )
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "podman",
            Arguments = command,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = false,
            WorkingDirectory = string.IsNullOrEmpty(workingDirectory)
                ? Directory.GetCurrentDirectory()
                : workingDirectory,
        };

        var process = new Process() { StartInfo = processStartInfo };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data == null)
            {
                return;
            }

            outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data == null)
            {
                return;
            }

            errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (!string.IsNullOrEmpty("prompt"))
        {
            // Write input to process (auto-accept prompts)
            await process.StandardInput.WriteLineAsync(prompt);
            await process.StandardInput.FlushAsync();
            process.StandardInput.Close(); // Important to close stdin
        }

        await process.WaitForExitAsync();

        return new(process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
    }
}
