using System.Text;
namespace spinner;


public class SequenceParser(params IParser[] parsers) : IParser
{
    public ParserType Type { get; set; } = ParserType.Sequence;
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        List<IToken> values = [];

        for (int i = 0; i < parsers.Length; i++)
        {
            var result = parsers[i].Parse(context);
            if (!result.Success)
            {
                int at = context.Position;
                context.Position = initialPosition;

                return ParseResult.FailAt(new ParseFailedToken(initialPosition, parsers[i]) { At = at });
            }

            if (result.Token is not DefaultToken && result.Token is not EOFToken)
            {
                values.Add(result.Token);
            }
        }

        return ParseResult.SuccessAt(new SequenceToken(values.ToArray()) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } });
    }
}

public struct SequenceToken(IToken[] childen) : IToken
{
    public IToken[] Children = childen;
    public Range Body { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<{ParserType.Sequence}>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</{ParserType.Sequence}>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
