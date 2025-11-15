namespace spiner;

public class LineBreakParser : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        if (!context.HasNext() || context.Input[context.Position] != '\n')
        {
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, this));
        }

        context.Position++;

        return ParseResult.SuccessAt(new LineBreakToken() { Body = new() { Start = initialPosition, Length = 1 } });

    }
}

public struct LineBreakToken : IToken
{
    public Range Body { get; init; }

    public string ToString(string source, int depth)
    {
        return $"{"".PadRight(4 * depth)}<br/>";
    }
}
