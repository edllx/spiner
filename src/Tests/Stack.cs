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

    public string? GetKey(string serviceName, string keyName)
    {
        if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(keyName))
        {
            return null;
        }

        Service? s = null;

        for (int i = 0; i < Services.Length; i++)
        {
            if (Services[i].Name == serviceName)
            {
                s = Services[i];
                break;
            }
        }
        if (s is null)
        {
            return null;
        }

        return s.Scope.Get(keyName);
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
