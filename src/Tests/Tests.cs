using System.Text;

namespace spinner;

public class Tests
{
    public static string DefaultMode { get; } = "sync";
    public string Description { get; init; }
    public Test[] TestSet { get; init; }
    public Scope Scope { get; init; }
    public string Mode { get; init; }
    public string Id { get; } = Tools.GenerateRandomString(20, "testset-");

    public Tests(
        Test[]? testSet = null,
        string? mode = null,
        Scope? scope = null,
        string description = ""
    )
    {
        TestSet = testSet ?? [];
        Mode = mode ?? "sync";
        Scope = scope ?? new();
        Description = description;
    }

    public Tests()
    {
        TestSet = [];
        Mode = DefaultMode;
        Scope = new();
        Description = "";
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<TestSet mode=\"{Mode}\">");
        if (Scope.Keys.Count > 0)
        {
            builder.Append($"\n{Scope.ToString(depth + 1)}");
        }

        if (TestSet.Length > 0)
        {
            builder.Append("\n");
            builder.Append(string.Join("\n", TestSet.Select(v => v.ToString(depth + 1))));
        }

        builder.Append($"\n{"".PadRight(4 * depth)}</TestSet>");

        return builder.ToString();
    }
}
