using System.Text;

namespace spiner;

public class AndParser(IParser first, IParser second) : IParser
{
    private IParser _first { get; init; } = first;
    private IParser _second { get; init; } = second;

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        ParseResult firstResult = _first.Parse(context);

        if (!firstResult.Success)
        {
            context.Position = initialPosition;
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, this));
        }

        ParseResult secondResult = _second.Parse(context);

        if (!secondResult.Success)
        {
            context.Position = initialPosition;
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, this));
        }

        ParseResult res = new ParseResult(true)
        {
            Token = new AndToken(firstResult.Token, secondResult.Token) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } },
        };

        return res;
    }
}

public struct AndToken(IToken first, IToken second) : IToken
{
    public IToken First = first;
    public IToken Second = second;
    public Range Body { get; init; }

    public string ToString(string source, int depth)
    {
        var buffer = new StringBuilder();

        buffer.Append(First.ToString(source, depth));
        buffer.Append(Second.ToString(source, depth));

        var lfMark = $"{"".PadRight(4 * depth)}<{ParserType.And}>\n";
        var rgMark = $"\n{"".PadRight(4 * depth)}</{ParserType.And}>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
