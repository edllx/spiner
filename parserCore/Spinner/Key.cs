using static spinner.Parser;

namespace spinner;

internal class KeyDetector : IParser
{
    private static IParser OpenningMark = Seq(Char('{'), Char('{'));
    private static IParser ClossingMark = Seq(Char('}'), Char('}'));
    private IParser Key = Seq(OpenningMark, ConsumeUntil(ClossingMark), ClossingMark);

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        var res = Key.Parse(context);
        if (!res.Success)
        {
            return ParseResult.FailAt(
                new ParseFailedToken(initialPosition, Key) { At = initialPosition }
            );
        }

        SequenceToken seq = (SequenceToken)res.Token;
        List<IToken> tk = [];
        KeyParser.Unroll(seq.Children[1], tk);

        return ParseResult.SuccessAt(
            new KeyToken()
            {
                Body = res.Token.Body,
                Name = new Range() { Start = tk[0].Body.Start, Length = tk[0].Body.Length },
            }
        );
    }
}

internal class KeySpliter : IParser
{
    private static IParser OpenningMark = Seq(Char('{'), Char('{'));
    private IParser Value = ZeroPlus(
        Seq(Optional(ConsumeUntil(OpenningMark)), Optional(KeyParser.SpinnerKey))
    );

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        var res = Value.Parse(context);

        List<IToken> tk = [];
        KeyParser.Unroll(res.Token, tk);

        return ParseResult.SuccessAt(new KeyTokens() { Tokens = tk.ToArray() });
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

public class KeyTokens : IToken
{
    public Range Body { get; init; }
    public IToken[] Tokens { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var lfMark = $"{"".PadRight(4 * depth)}<Key>\n";
        var rgMark = $"\n{"".PadRight(4 * depth)}</Key>";

        var body =
            $"{lfMark}{string.Join("\n", Tokens.Select(val => val.ToString(source, depth + 1)))}{rgMark}";
        return body;
    }
}

public class KeyToken : IToken
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
