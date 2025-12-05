using System.Text;

namespace spinner;

public enum ServiceState
{
    Uninitialized,
    Stoped,
    Running,
    Disposed,
}

public class Service
{
    public string Name { get; init; }
    public string Id { get; init; }
    public string Image { get; init; }
    public string BuildPath { get; init; }
    public Scope Scope { get; init; }
    public IRun[] Command { get; init; }

    public Service(
        string name,
        string id,
        string? image = null,
        string? buildPath = null,
        Scope? scope = null,
        IRun[]? commands = null
    )
    {
        Name = name;
        Id = id;
        Image = image ?? "";
        BuildPath = buildPath ?? "";
        Scope = scope ?? new();
        Command = commands ?? [];
    }

    public Service()
    {
        Name = "";
        Id = "";
        Image = "";
        BuildPath = "";
        Scope = new();
        Command = [];
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();
        var image = string.IsNullOrEmpty(Image) ? "" : $" image=\"{Image}\"";
        var build = string.IsNullOrEmpty(BuildPath) ? "" : $" build=\"{BuildPath}\"";

        builder.Append($"{"".PadRight(4 * depth)}<Service name=\"{Name}\"{image}{build}>\n");
        builder.Append($"{Scope.ToString(depth + 1)}");
        if (Command.Length > 0)
        {
            builder.Append($"\n{"".PadRight(4 * (depth + 1))}<Layer>");
            builder.Append($"\n{string.Join("\n", Command.Select(v => v.ToString(depth + 2)))}");
            builder.Append($"\n{"".PadRight(4 * (depth + 1))}</Layer>");
        }
        builder.Append($"\n{"".PadRight(4 * depth)}</Service>");

        return builder.ToString();
    }
}
