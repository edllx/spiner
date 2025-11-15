namespace spiner;


public class CharParser(char c) : IParser
{
    public char C => c;
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        if (!context.HasNext() || context.Input[context.Position] != c)
        {
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, this));
        }

        context.Position++;

        return ParseResult.SuccessAt(new TextToken() { Body = new() { Start = initialPosition, Length = 1 } });
    }
}


