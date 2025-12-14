using System.Text;

namespace spinner;

public enum ServiceState
{
    Uninitialized,
    Stoped,
    Running,
    Disposed,
}

public interface IService
{
    Task<IService> Init();
    Task<Service> Build();
}

public class Service
{
    public string Name { get; init; }
    public string Image { get; init; }
    public bool Target { get; init; } = false;
    public int Port { get; init; } = 3200;
    public Scope Scope { get; init; }
    public IRun[] Commands { get; private init; }

    public Arg[] Args { get; init; }

    public Service(
        string name,
        string image,
        string? buildPath = null,
        bool? target = null,
        Scope? scope = null,
        IRun[]? commands = null,
        Arg[]? args = null
    )
    {
        Name = name;
        Image = image;
        Target = target ?? false;
        Scope = scope ?? new();
        Commands = commands ?? [];
        Args = args ?? [];
    }

    public Service()
    {
        Name = "";
        Image = "";
        Target = false;
        Scope = new();
        Commands = [];
        Args = [];
    }

    public void ApplyArgs(Stack? stack = null)
    {
        for (int i = 0; i < Args.Length; i++)
        {
            var arg = Args[i];
            string value = arg.Value;
            if (stack is not null && !string.IsNullOrEmpty(arg.FROM))
            {
                var v = stack.GetKey(arg.FROM, arg.Key);
                value = v is not null ? v : value;
            }

            Scope.Set(arg.Key, value, bubble: false, create: false);
        }
    }

    public void ResolveLayer()
    {
        for (int i = 0; i < Commands.Length; i++)
        {
            try
            {
                switch (Commands[i])
                {
                    case Copy cp:
                        cp.Source = KeyManager.Resolve(cp.Source, Scope.Keys);
                        cp.Destination = KeyManager.Resolve(cp.Destination, Scope.Keys);
                        break;

                    case Run r:

                        r.Text = KeyManager.Resolve(r.Text, Scope.Keys);
                        break;
                }
            }
            catch (Exception) { }
        }
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();
        var image = string.IsNullOrEmpty(Image) ? "" : $" image=\"{Image}\"";
        var exposed = Target ? " target" : "";

        builder.Append($"{"".PadRight(4 * depth)}<Service name=\"{Name}\"{image}{exposed}>\n");
        builder.Append($"{Scope.ToString(depth + 1)}");
        if (Commands.Length > 0)
        {
            builder.Append($"\n{"".PadRight(4 * (depth + 1))}<Layer>");
            builder.Append($"\n{string.Join("\n", Commands.Select(v => v.ToString(depth + 2)))}");
            builder.Append($"\n{"".PadRight(4 * (depth + 1))}</Layer>");
        }
        builder.Append($"\n{"".PadRight(4 * depth)}</Service>");

        return builder.ToString();
    }
}
