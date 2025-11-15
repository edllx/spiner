namespace spiner;

public class OnePlusParser(IParser parser) : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        bool success = true;

        List<IToken> values = [];
        ParseResult result = parser.Parse(context);


        while (context.HasNext() && result.Success)
        {
            values.Add(result.Token);
            result = parser.Parse(context);
        }

        if (result.Success) { values.Add(result.Token); }

        if (values.Count < 1)
        {
            context.Position = initialPosition;
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, parser));
        }

        if (!success) { context.Position = initialPosition; }

        return ParseResult.SuccessAt(new SequenceToken(values.ToArray()) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } });
    }
}
