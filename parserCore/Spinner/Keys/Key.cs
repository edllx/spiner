namespace spinner;

public class GenerationInfo
{
    public int Len { get; init; } = 20;
    public int Seed { get; init; } = -1;
    public string Prefix { get; init; } = "";

    public GenerationInfo Copy()
    {
        return new()
        {
            Len = Len,
            Seed = Seed,
            Prefix = Prefix,
        };
    }
}

public class Key
{
    public string Name { get; init; }
    public string Value { get; set; }
    public bool Generated { get; init; }
    public GenerationInfo GenInfo { get; init; } = new();

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

    public bool Resolve()
    {
        if (Generated)
        {
            Value = Tools.GenerateRandomString(GenInfo);
            return true;
        }
        return false;
    }

    public bool Resolve(string value)
    {
        if (Resolve())
        {
            return true;
        }

        Value = value;
        return false;
    }

    public bool Resolve(IEnumerable<Key> keys)
    {
        if (Resolve())
        {
            return true;
        }

        try
        {
            var value = KeyManager.Resolve(Name, keys);
            return Resolve(value);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool Resolve(Scope scope)
    {
        if (Resolve())
        {
            return true;
        }

        return Resolve(scope.Keys);
    }

    public override string ToString()
    {
        return $"{Name} : {Value}";
    }

    public Key Copy()
    {
        return new(Name, Value) { Generated = Generated, GenInfo = GenInfo.Copy() };
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
