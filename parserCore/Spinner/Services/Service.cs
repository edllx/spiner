using System.Text;
using static spinner.Parser;

namespace spinner;

internal class ServiceParser : IParser
{
    private static IParser ClosingTag = Seq(StringP("</"), StringP("Service"), Char('>'));
    private static IParser Comment = new XMLCommentParser();
    private static IParser GenericXMLElement = new XMLElemenParser(AlphaChar);
    private static IParser Keys = new SpinnerKeyParser();
    private static IParser Layers = new ServiceLayerParser();

    private static IParser Body = ZeroPlus(
        Choice(LineBreak, Comment, Keys, Layers, GenericXMLElement, ConsumeUntil(ClosingTag))
    );
    private static IParser Spaces = AnyStringP(" \t");

    private static IParser ServiceBody = new XMLElemenParser("Service", Body);
    private static IParser Service = Seq(Optional(Spaces), ServiceBody);

    public ParseResult Parse(ParseContext context)
    {
        var res = Service.Parse(context);
        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;
        XMLElementToken xmlElement = (XMLElementToken)seq.Children[1];

        return ParseResult.SuccessAt(
            new ServiceToken()
            {
                Body = xmlElement.Body,
                Attributes = xmlElement.Attributes,
                Children = xmlElement.Children,
            }
        );
    }
}

public class ServiceToken : IToken
{
    public IToken[] Children { get; init; } = [];
    public XMLAttributeToken[] Attributes { get; init; } = [];
    public Range Body { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Service>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Service>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
