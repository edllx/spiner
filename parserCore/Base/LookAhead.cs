namespace spiner;

public class PositiveLookAheadParser(IParser parser) : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        var result = parser.Parse(context);
        context.Position = initialPosition;

        if (result.Success)
        {
            return ParseResult.SuccessAt(new LookAheadToken(result.Token));
        }

        return ParseResult.FailAt(new ParseFailedToken(initialPosition, parser));
    }
}

public class NegativeLookAheadParser(IParser parser) : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        var result = parser.Parse(context);
        context.Position = initialPosition;

        if (!result.Success)
        {
            return ParseResult.SuccessAt(new LookAheadToken(result.Token));
        }

        return ParseResult.FailAt(new ParseFailedToken(initialPosition, parser));
    }
}


public struct LookAheadToken(IToken token) : IToken
{
    public Range Body { get; init; }
    public IToken Token { get; init; } = token;

    public string ToString(string source, int depth)
    {
        return Token.ToString(source, depth);
    }
}
