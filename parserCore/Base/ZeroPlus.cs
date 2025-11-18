namespace spinner;

public class ZeroPlusParser(IParser parser) : IParser
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
        if (!success) { context.Position = initialPosition; }

        var x = new SequenceToken(values.ToArray()) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } };
        return ParseResult.SuccessAt(new SequenceToken(values.ToArray()) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } });
    }
}
