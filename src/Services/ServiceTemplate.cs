using System.Text;

namespace spinner;

public partial class ServiceTemplate
{
    public string Name { get; init; }
    public string? Image { get; init; }
    public string? BuildPath { get; init; }
    public Layer[] Layers { get; init; } = [];
    public Scope Scope { get; init; }

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
