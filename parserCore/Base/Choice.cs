namespace spinner;

public class ChoiceParser(params IParser[] parsers) : IParser
{

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        for (int i = 0; i < parsers.Length; i++)
        {
            var result = parsers[i].Parse(context);
            if (result.Success) { return ParseResult.SuccessAt(result.Token); }
        }

        context.Position = initialPosition;
        return ParseResult.FailAt(new ParseFailedToken(initialPosition, this) { Body = new() { Start = initialPosition } });
    }
}
