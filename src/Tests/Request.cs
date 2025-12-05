using System.Text;

namespace spinner;

public class TestRequest
{
    public Scope Scope { get; init; }
    public string Path { get; init; } = "";
    public string Method { get; init; } = "";
    public RequestBody? Body { get; init; }

    public TestRequest(
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
    }

    public TestRequest()
    {
        Scope = new();
        Path = "";
        Method = "GET";
        Body = null;
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<Request path=\"{Path}\" >");

        if (Scope.Keys.Count > 0)
        {
            builder.Append($"\n{Scope.ToString(depth + 1)}");
        }

        if (Body is not null && Body.Keys.Length > 0)
        {
            builder.Append($"\n{Body.ToString(depth + 1)}");
        }

        builder.Append($"\n{"".PadRight(4 * depth)}</Request>");

        return builder.ToString();
    }
}
