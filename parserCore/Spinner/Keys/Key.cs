namespace spinner;

public struct GenerationInfo
{
    public int Len { get; init; }
}

public class Key
{
    public string Name { get; init; }
    public string Value { get; set; }
    public bool Resolved { get; private set; }
    public bool Generated { get; init; }
    public GenerationInfo GenInfo { get; init; }

    public Key()
    {
        Name = "";
        Value = "";
    }

    public Key(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public void Set(string value)
    {
        Value = value;
    }

    public void Resolve(string value)
    {
        if (Resolved)
        {
            return;
        }

        Value = value;

        Resolved = true;
    }

    public override string ToString()
    {
        return $"{Name} : {Value}";
    }

    public static Key Build(IToken token, string source)
    {
        if (token is not SpinnerToken tk)
        {
            throw new Exception("THis is not a valid Layer token");
        }

        var name = "";
        var value = "";
        var len = "";

        name =
            tk.Attributes.FirstOrDefault(v => v.Name.ToString(source) == "name")
                ?.Value.ToString(source)
            ?? "";

        value =
            tk.Attributes.FirstOrDefault(v => v.Name.ToString(source) == "value")
                ?.Value.ToString(source)
            ?? "";

        len =
            tk.Attributes.FirstOrDefault(v => v.Name.ToString(source) == "len")
                ?.Value.ToString(source)
            ?? "";

        if (string.IsNullOrEmpty(name))
        {
            throw new Exception("Emty Key name");
        }

        switch (tk.Name)
        {
            case "Key":
                return new(name, value);

            case "GeneratedKey":
                if (!int.TryParse(len, out int ln))
                {
                    throw new Exception("Invalid generated key len");
                }
                value = Tools.GenerateRandomString(ln);
                return new(name, "{{Generated}}")
                {
                    Generated = true,
                    GenInfo = new() { Len = ln },
                };
        }

        throw new Exception("Invalid key");
    }
}

public class KeyParser
{
    public static readonly IParser SpinnerKey = new KeyDetector();
    public static readonly IParser SpinnerKeyPart = new KeySpliter();

    public ParseResult Parse(string source)
    {
        var context = new ParseContext(source);
        return SpinnerKeyPart.Parse(context);
    }

    public static void Unroll(IToken token, List<IToken> destination)
    {
        switch (token)
        {
            case TextToken text:
                destination.Add(text);
                break;

            case SequenceToken seq:
                foreach (IToken t in seq.Children)
                {
                    Unroll(t, destination);
                }
                break;

            case DefaultToken def:
                break;
            default:
                destination.Add(token);
                break;
        }
    }
}

public class KeyToken : IToken
{
    public Range Body { get; init; }
    public IToken[] Tokens { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var lfMark = $"{"".PadRight(4 * depth)}<Key>\n";
        var body = string.Join("\n", Tokens.Select(val => val.ToString(source, depth + 1)));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Key>";

        return $"{lfMark}{body}{rgMark}";
    }
}

public class KeyRefToken : IToken
{
    public Range Body { get; init; }
    public Range Name { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var key =
            $"{"".PadRight(4 * depth)}<Ref name='{source.AsSpan().Slice(Name.Start, Name.Length)}'/>";
        return key;
    }
}
