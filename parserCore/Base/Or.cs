namespace spinner;

public class OrParser(IParser first, IParser second) : IParser
{
    private IParser _first { get; init; } = first;
    private IParser _second { get; init; } = second;

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        ParseResult firstResult = _first.Parse(context);
        if (firstResult.Success) { return ParseResult.SuccessAt(new OrToken(this) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } }); }

        ParseResult secondResult = _second.Parse(context);
        if (secondResult.Success) { return ParseResult.SuccessAt(new OrToken(this) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } }); }

        return ParseResult.FailAt(new ParseFailedToken(initialPosition, this));
    }
}


public struct OrToken(IParser parser) : IToken
{
    public IParser Parser = parser;
    public Range Body { get; init; }

    public string ToString(string source, int depth)
    {
        return $"{"".PadRight(4 * depth)}<Or First='{Parser.GetType().Name}' />";
    }
}


