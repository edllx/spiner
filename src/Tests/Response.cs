using System.Text;

namespace spinner;

public class TestResponse
{
    public Setter[] Setters { get; init; }

    public TestResponse(Setter[] setters)
    {
        Setters = setters;
    }

    public TestResponse()
    {
        Setters = [];
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<Response>");
        builder.Append($"\n{string.Join("\n", Setters.Select(v => v.ToString(depth + 1)))}");
        builder.Append($"\n{"".PadRight(4 * depth)}</Response>");

        return builder.ToString();
    }
}

public class Setter
{
    public Setter(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public Setter()
    {
        Key = "";
        Value = "";
    }

    public string Key { get; init; }
    public string Value { get; init; }

    public string ToString(int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<Set key=\"{Key}\" value=\"{Value}\" />";
    }
}
