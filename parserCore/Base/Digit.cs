namespace spiner;

public class DigitParser : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        if (!context.HasNext())
        {
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, this));
        }

        bool isDigit = char.IsAsciiDigit(context.Input[context.Position]);

        if (!isDigit)
        {
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, this));
        }

        context.Position++;

        return ParseResult.SuccessAt(new DigitToken() { Body = new() { Start = initialPosition, Length = 1 } });
    }
}

public struct DigitToken : IToken
{
    public Range Body { get; init; }

    public string ToString(string source, int depth)
    {
        return $"{"".PadRight(4 * depth)}<Digit Content='{source.AsSpan().Slice(Body.Start, Body.Length)}'/>";
    }
}
