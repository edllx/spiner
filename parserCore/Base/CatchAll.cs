namespace spinner;

public class CatchAllParser() : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        if (!context.HasNext())
        {
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, this));
        }

        while (context.Position < context.Input.Length)
        {
            context.Position++;
        }

        var ret = ParseResult.SuccessAt(
            new TextToken()
            {
                Body = new()
                {
                    Start = initialPosition,
                    Length = context.Position - initialPosition,
                },
            }
        );

        return ret;
    }
}
