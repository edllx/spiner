namespace spinner;

public interface IInitializable
{
    Task Initialize(Service service);
}

public class ServiceImage : IInitializable
{
    public async Task Initialize(Service service)
    {
        await Task.Delay(4000);
        Console.WriteLine($"Service: {service.Name} Initialized");
    }
}

public class ServiceBuilder : IInitializable
{
    public async Task Initialize(Service service)
    {
        await Task.Delay(4000);
        Console.WriteLine($"Service: {service.Name} Initialized");
    }
}

public enum ServiceState
{
    Uninitialized,
    Stoped,
    Running,
    Disposed,
}

public class Service : IDisposable
{
    public required string Name { get; init; }
    public required string Image { get; init; }
    public Scope Scope { get; init; }
    private bool IsLayerApplyed;
    private ServiceState State = ServiceState.Uninitialized;

    public Service(Scope parentScope)
    {
        Scope = new() { Parent = parentScope };
    }

    public Service()
    {
        Scope = new();
    }

    private async Task Init()
    {
        if (State != ServiceState.Uninitialized)
        {
            return;
        }

        // build container
        Console.WriteLine("Building Container");

        State = ServiceState.Stoped;
    }

    public async Task ApplyLayer(Layer layer)
    {
        if (IsLayerApplyed)
        {
            return;
        }

        Console.WriteLine("Appling layer");

        // execute setup scripts
    }

    public async Task Lauch()
    {
        if (State == ServiceState.Running)
        {
            return;
        }

        Console.WriteLine("Launching container");

        // start container

        State = ServiceState.Running;
    }

    public async Task Stop()
    {
        if (State == ServiceState.Stoped)
        {
            return;
        }

        // stop container

        State = ServiceState.Stoped;
    }

    public async Task Clean()
    {
        if (State == ServiceState.Disposed)
        {
            return;
        }

        await Stop();

        // delete container

        State = ServiceState.Disposed;
    }

    public void Dispose()
    {
        _ = Clean();
    }
}
