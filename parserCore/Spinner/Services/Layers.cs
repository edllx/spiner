using System.Text;
using static spinner.Parser;

namespace spinner;

public class SpinnerLayerParser : IParser
{
    private static IParser ClosingTag = Seq(StringP("</"), StringP("Layer"), Char('>'));
    private static IParser Comment = new XMLCommentParser();
    private static IParser GenericXMLElement = new XMLElemenParser(AlphaChar);
    private static IParser Keys = new SpinnerKeyParser();
    private static IParser Copy = new SpinnerCopy();
    private static IParser Run = new SpinnerRun();
    private static IParser SQL = new SpinnerSqlParser();

    private static IParser Body = ZeroPlus(
        Choice(
            LineBreak,
            Comment,
            Keys,
            Copy,
            Run,
            SQL,
            GenericXMLElement,
            ConsumeUntil(ClosingTag)
        )
    );
    private static IParser Spaces = AnyStringP(" \t");

    private static IParser ServiceBody = new XMLElemenParser("Layer", Body);
    private static IParser Service = Seq(Optional(Spaces), ServiceBody);

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        var res = Service.Parse(context);
        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;
        XMLElementToken xmlElement = (XMLElementToken)seq.Children[1];

        return ParseResult.SuccessAt(
            new SpinnerLayerToken()
            {
                Body = xmlElement.Body,
                Attributes = xmlElement.Attributes,
                Children = xmlElement.Children,
            }
        );
    }
}

public class SpinnerLayerToken : IToken
{
    public IToken[] Children { get; init; } = [];
    public XMLAttributeToken[] Attributes { get; init; } = [];
    public Range Body { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Layer>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Layer>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
