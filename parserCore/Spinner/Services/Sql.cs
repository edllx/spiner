using static spinner.Parser;

namespace spinner;

public class SpinnerSqlParser : IParser
{
    private static IParser SQL = new XMLSingleLineElementParser("Sql");

    private static IParser Spaces = AnyStringP(" \t");
    private static IParser Element = Seq(Optional(Spaces), SQL);

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

        Range source = new();

        for (int i = 0; i < xmlElement.Attributes.Length; i++)
        {
            var att = xmlElement.Attributes[i];
            switch (context.Input.AsSpan().Slice(att.Name.Start, att.Name.Length).ToString())
            {
                case "source":
                    source = att.Value;
                    break;

                default:
                    break;
            }
        }

        if (source.Length <= 0)
        {
            throw new MissingKeyAttributeException(initialPosition);
        }

        return ParseResult.SuccessAt(new SpinnerSqlToken() { Body = seq.Body, Source = source });
    }
}

public class SpinnerSqlToken : IToken
{
    public Range Body { get; init; }
    public Range Source { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var src = $"source=\"{source.AsSpan().Slice(Source.Start, Source.Length)}\"";
        return $"{"".PadRight(4 * depth)}<Sql {src}/>";
    }
}
