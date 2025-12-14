namespace spinner;

public class AnyParser : IParser
{
    public ParseResult Parse(ParseContext context)
    {

        int initialPosition = context.Position;
        if (!context.HasNext()) { return ParseResult.FailAt(new ParseFailedToken(initialPosition, this)); }

        context.Position++;

        return ParseResult.SuccessAt(new TextToken() { Body = new() { Start = initialPosition, Length = 1 } });
    }
}
