using System.Text;

namespace spinner;

public interface IServiceTemplate
{
    Task<IServiceTemplate> Init();
    Task<IServiceTemplate> ApplyLayer(Layer l);
    Task<Service> Build();
}

public partial class ServiceTemplate : IServiceTemplate, IDisposable
{
    public string Name { get; init; }
    public string? Image { get; init; }
    public string? BuildPath { get; init; }
    public Layer[] Layers { get; init; } = [];
    public Scope Scope { get; init; }
    private bool IsLayerApplyed = false;
    private ServiceState State = ServiceState.Uninitialized;

    public ServiceTemplate(
        string name,
        string? image = null,
        string? buildPath = null,
        Scope? scope = null,
        Layer[]? layers = null
    )
    {
        Name = name;
        Image = image;
        BuildPath = buildPath;
        Scope = scope ?? new();
        Layers = layers ?? [];
    }

    public ServiceTemplate()
    {
        Name = "";
        Image = "";
        BuildPath = "";
        Scope = new();
        Layers = [];
    }

    public async Task<IServiceTemplate> Init()
    {
        if (State != ServiceState.Uninitialized)
        {
            return this;
        }

        // build container
        Console.WriteLine("Building Container");

        State = ServiceState.Stoped;
        await Task.CompletedTask;
        return this;
    }

    public async Task<IServiceTemplate> ApplyLayer(Layer layer)
    {
        if (IsLayerApplyed)
        {
            return this;
        }

        Console.WriteLine("Appling layer");

        await Task.CompletedTask;
        // execute setup scripts
        return this;
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
        await Task.CompletedTask;
    }

    public async Task Stop()
    {
        if (State == ServiceState.Stoped)
        {
            return;
        }

        // stop container

        State = ServiceState.Stoped;
        await Task.CompletedTask;
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
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _ = Clean();
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();
        var image = string.IsNullOrEmpty(Image) ? "" : $" image=\"{Image}\"";
        var build = string.IsNullOrEmpty(BuildPath) ? "" : $" build=\"{BuildPath}\"";

        builder.Append(
            $"{"".PadRight(4 * depth)}<ServiceTemplate name=\"{Name}\"{image}{build}>\n"
        );
        builder.Append($"{Scope.ToString(depth + 1)}");
        if (Layers.Length > 0)
        {
            builder.Append($"\n{string.Join("\n", Layers.Select(v => v.ToString(depth + 1)))}");
        }
        builder.Append($"\n{"".PadRight(4 * depth)}</ServiceTemplate>");

        return builder.ToString();
    }

    public Task<Service> Build()
    {
        throw new NotImplementedException();
    }
}
