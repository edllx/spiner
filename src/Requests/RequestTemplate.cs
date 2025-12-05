using System.Text;

namespace spinner;

public partial class RequestTemplate
{
    public RequestTemplate(
        string name,
        string? method = null,
        Scope? scope = null,
        string? path = null,
        RequestBody? body = null
    )
    {
        Name = name;
        Scope = scope ?? new();
        Method = method ?? "GET";
        Path = path ?? "";
        Body = body ?? new();
    }

    public RequestTemplate()
    {
        Name = "";
        Scope = new();
        Method = "GET";
        Path = "";
        Body = new();
    }

    public string Name { get; init; }
    public Scope Scope { get; init; }
    public string Method { get; init; }
    public string Path { get; init; }
    public RequestBody Body { get; init; }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();
        var path = string.IsNullOrEmpty(Path) ? "" : $" path=\"{Path}\"";
        var method = string.IsNullOrEmpty(Method) ? "" : $" method=\"{Method}\"";

        builder.Append($"{"".PadRight(4 * depth)}<RequestTemplate name=\"{Name}\"{method}{path}>");

        if (Scope.Keys.Count > 0)
        {
            builder.Append($"\n{Scope.ToString(depth + 1)}");
        }

        if (Body.Keys.Length > 0)
        {
            builder.Append($"\n{Body.ToString(depth + 1)}");
        }

        builder.Append($"\n{"".PadRight(4 * depth)}</RequestTemplate>");

        return builder.ToString();
    }
}
