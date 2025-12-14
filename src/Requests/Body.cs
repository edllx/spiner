using System.Text;

namespace spinner;

public class RequestBody : Iresovable
{
    public static string DefaurlType = "json";
    public string Type { get; init; }
    public Key[] Keys { get; init; }

    public RequestBody(string? type = null, Key[]? keys = null)
    {
        Type = type ?? "json";
        Keys = keys ?? [];
    }

    public RequestBody(RequestBody body)
    {
        Type = body.Type;
        Keys = body.Keys.Select(v => new Key(v.Name, v.Value)).ToArray();
    }

    public RequestBody()
    {
        Type = DefaurlType;
        Keys = [];
    }

    public RequestBody Copy()
    {
        return new(this);
    }

    public object? Model()
    {
        var res = new Dictionary<string, string>();
        foreach (Key item in Keys)
        {
            res.Add(item.Name, item.Value);
        }
        return res;
    }

    public string ToString(int depth)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<Body>\n");
        builder.Append(
            string.Join(
                "\n",
                Keys.Select(v =>
                {
                    return $"{"".PadRight(4 * (depth + 1))}{v.Name}: \"{v.Value}\"";
                })
            )
        );
        builder.Append($"\n{"".PadRight(4 * depth)}</Body>");

        return builder.ToString();
    }

    public void Resolve(Scope? scope = null)
    {
        if (scope is null)
        {
            return;
        }

        for (int i = 0; i < Keys.Length; i++)
        {
            var str = KeyManager.Resolve(Keys[i].Value, scope);
            if (str is null)
            {
                continue;
            }
            Keys[i].Resolve(str);
        }
    }
}
