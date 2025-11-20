namespace spinner;

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

public class KeysToken : IToken
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
