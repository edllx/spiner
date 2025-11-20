using static spinner.Parser;

namespace spinner;

public class SpinnerCopy : IParser
{
    private static IParser Copy = new XMLSingleLineElementParser("Copy");

    private static IParser Spaces = AnyStringP(" \t");
    private static IParser Element = Seq(Optional(Spaces), Copy);

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        var res = Element.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;
        XMLElementToken xmlElement = (XMLElementToken)seq.Children[1];

        Range from = new();
        Range to = new();

        for (int i = 0; i < xmlElement.Attributes.Length; i++)
        {
            var att = xmlElement.Attributes[i];
            switch (context.Input.AsSpan().Slice(att.Name.Start, att.Name.Length).ToString())
            {
                case "source":
                    from = att.Value;
                    break;

                case "dest":
                    to = att.Value;
                    break;
                default:
                    break;
            }
        }

        return ParseResult.SuccessAt(
            new SpinnerCopyToken()
            {
                Body = seq.Body,
                Source = from,
                Dest = to,
            }
        );
    }
}

public class SpinnerCopyToken : IToken
{
    public Range Body { get; init; }
    public Range Source { get; init; }
    public Range Dest { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var from = $"source=\"{source.AsSpan().Slice(Source.Start, Source.Length)}\"";
        var to = $"dest=\"{source.AsSpan().Slice(Dest.Start, Dest.Length)}\"";
        return $"{"".PadRight(4 * depth)}<Copy {from} {to}/>";
    }
}
