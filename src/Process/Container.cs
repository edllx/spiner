namespace spinner;

public interface IContainerService
{
    Task RunContainerAsync(
        string image,
        string name,
        (string, string)[] envVariables,
        (int, int)[] ports,
        bool replace = true
    );
    Task RunContainerAsync(string image, string name, bool replace = false);
    Task StopContainerAsync(string containerId);
    Task RemoveContainerAsync(string containerId, bool force = false);

    Task BuildImageAsync(
        string buildFilePath,
        string contextPath,
        string tag,
        CancellationToken? token = null
    );
    Task ExecCommandAsync(string containerId, string command, CancellationToken? token = null);
    Task IsContainerHealthyAsync(string containerId);
}
