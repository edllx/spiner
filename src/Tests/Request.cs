using System.Text;

namespace spinner;

public class TestRequest : Iresovable
{
    public string Name { get; init; }
    public Scope Scope { get; init; }
    public string Path { get; set; } = "";
    public string Method { get; init; } = "";
    public RequestBody? Body { get; init; }

    public TestRequest(
        string name = "",
        string? path = "",
        string? method = "GET",
        Scope? scope = null,
        RequestBody? body = null
    )
    {
        Scope = scope ?? new();
        Path = path ?? "";
        Method = method ?? "GET";
        Body = body;
        Name = name;
    }

    public TestRequest()
    {
        Scope = new();
        Path = "";
        Method = "GET";
        Body = null;
        Name = "default";
    }

    public void Resolve(Scope? scope = null)
    {
        var s = scope ?? Scope;
        Path = KeyManager.Resolve(Path, s);
        Body?.Resolve(s);
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        if (Body is null || Body.Keys.Length == 0)
        {
            return $"{"".PadRight(4 * depth)}<Request name=\"{Name}\" method=\"{Method}\" path=\"{Path}\" />";
        }

        builder.Append(
            $"{"".PadRight(4 * depth)}<Request name=\"{Name}\" method=\"{Method}\" path=\"{Path}\" >"
        );

        if (Body is not null && Body.Keys.Length > 0)
        {
            builder.Append($"\n{Body.ToString(depth + 1)}");
        }

        builder.Append($"\n{"".PadRight(4 * depth)}</Request>");

        return builder.ToString();
    }
}
