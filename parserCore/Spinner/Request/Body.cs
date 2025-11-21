using System.Text;
using static spinner.Parser;

namespace spinner;

public class RequestBodyParser : IParser
{
    private static IParser ClosingTag = Seq(StringP("</"), StringP("Body"), Char('>'));
    private static IParser Comment = new XMLCommentParser();
    private static IParser GenericXMLElement = new XMLElemenParser(AlphaChar);
    private static IParser Keys = new SpinnerKeyParser();

    private static IParser Body = ZeroPlus(
        Choice(LineBreak, Comment, Keys, GenericXMLElement, ConsumeUntil(ClosingTag))
    );
    private static IParser Spaces = AnyStringP(" \t");

    private static IParser BBody = new XMLElemenParser("Body", Body);
    private static IParser Element = Seq(Optional(Spaces), BBody);

    public ParseResult Parse(ParseContext context)
    {
        var res = Element.Parse(context);
        if (!res.Success)
        {
            var token = (ParseFailedToken)res.Token;
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;
        XMLElementToken xmlElement = (XMLElementToken)seq.Children[1];

        return ParseResult.SuccessAt(
            new RequestBodyToken()
            {
                Attributes = xmlElement.Attributes,
                Body = xmlElement.Body,
                Children = xmlElement.Children,
            }
        );
    }
}

public class RequestBodyToken : IToken
{
    public Range Body { get; init; }
    public XMLAttributeToken[] Attributes { get; init; } = [];
    public IToken[] Children { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Body>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Body>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
