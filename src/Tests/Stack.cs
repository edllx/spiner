using System.Text;

namespace spinner;

public partial class Stack
{
    public Service[] Services { get; init; } = [];

    public Stack(Service[] services)
    {
        Services = services;
    }

    public Stack()
    {
        Services = [];
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<Stack>\n");
        builder.Append(string.Join("\n", Services.Select(v => v.ToString(depth + 1))));
        builder.Append($"\n{"".PadRight(4 * depth)}</Stack>");

        return builder.ToString();
    }
}
