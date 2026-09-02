using System.Text;
using static spinner.Parser;

namespace spinner;

public class XMLAttributeParser : IParser
{
    private static IParser Attribute = new XMLAttributeDetector();
    private static IParser Spaces = StringP(" ");
    private static IParser Attributes = ZeroPlus(
        Seq(OnePlus(Choice(Spaces, LineBreak)), Attribute)
    );

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        List<XMLAttributeToken> attributes = [];

        var res = Attributes.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        Unroll(res.Token, attributes);

        var token = new XMLAttributesToken()
        {
            Body = new() { Start = initialPosition, Length = context.Position - initialPosition },
            Tokens = attributes.ToArray(),
        };

        return ParseResult.SuccessAt(token);
    }

    private static void Unroll(IToken token, List<XMLAttributeToken> destination)
    {
        switch (token)
        {
            case XMLAttributeToken tk:
                destination.Add(tk);
                break;

            case SequenceToken seq:
                foreach (IToken t in seq.Children)
                {
                    Unroll(t, destination);
                }
                break;

            default:
                break;
        }
    }

    private class XMLAttributeDetector : IParser
    {
        private static IParser Attribute = Seq(
            AlphaChar,
            StringP("=\""),
            Choice(PrintableChar("\""), Empty),
            Char('"')
        );

        public ParseResult Parse(ParseContext context)
        {
            int initialPosition = context.Position;
            var res = Attribute.Parse(context);

            if (!res.Success)
            {
                if (!res.Success)
                {
                    var token = (ParseFailedToken)res.Token;
                    return res;
                }

                return res;
            }

            SequenceToken seq = (SequenceToken)res.Token;

            return ParseResult.SuccessAt(
                new XMLAttributeToken()
                {
                    Body = new()
                    {
                        Start = initialPosition,
                        Length = context.Position - initialPosition,
                    },
                    Name = seq.Children[0].Body,
                    Value = seq.Children[2].Body,
                }
            );
        }
    }
}

public class XMLAttributeToken : IToken
{
    public Range Body { get; init; }
    public Range Name { get; init; }
    public Range Value { get; init; }

    public string ToString(string source, int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<Attribute name=\"{source.AsSpan().Slice(Name.Start, Name.Length)}\" value=\"{source.AsSpan().Slice(Value.Start, Value.Length)}\"/>";
    }
}

public class XMLAttributesToken : IToken
{
    public Range Body { get; init; }
    public XMLAttributeToken[] Tokens { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Attributes>\n";
        buffer.Append(string.Join('\n', Tokens.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Attributes>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
