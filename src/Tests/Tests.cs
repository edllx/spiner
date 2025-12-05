using System.Text;

namespace spinner;

public class Tests
{
    public static string DefaultMode { get; } = "sync";
    private Test[] TestSet;
    public Scope Scope { get; init; }
    public string Mode { get; init; }

    public Tests(Test[]? testSet = null, string? mode = null, Scope? scope = null)
    {
        TestSet = testSet ?? [];
        Mode = mode ?? "sync";
        Scope = scope ?? new();
    }

    public Tests()
    {
        TestSet = [];
        Mode = DefaultMode;
        Scope = new();
    }

    private static void GetKeys(SpinnerToken token, string source, Scope scope)
    {
        if (token.Name != "Tests")
        {
            return;
        }

        for (int i = 0; i < token.Children.Length; i++)
        {
            var child = token.Children[i];

            try
            {
                var key = Key.Build(child, source);
                scope.Set((key.Name, key.Value));
            }
            catch (Exception) { }
        }
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
