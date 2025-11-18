namespace spinner;


public class EndOfFileParser : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        if (!context.HasNext()) { return ParseResult.SuccessAt(new EOFToken() { Body = new() { Start = initialPosition } }); }
        return ParseResult.FailAt(new ParseFailedToken(initialPosition, this));
    }
}

public struct EOFToken : IToken
{
    public Range Body { get; init; }

    public string ToString(string source, int depth)
    {
        return $"{"".PadRight(4 * depth)}EOF";
    }
}
