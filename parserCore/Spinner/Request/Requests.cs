using System.Text;
using static spinner.Parser;

namespace spinner;

public class RequestsParser : IParser
{
    private static IParser ClosingTag = Seq(StringP("</"), StringP("Requests"), Char('>'));
    private static IParser Comment = new XMLCommentParser();
    private static IParser GenericXMLElement = new XMLElemenParser(AlphaChar);
    private static IParser Keys = new SpinnerKeyParser();
    private static IParser Request = new RequestParser();

    private static IParser Body = ZeroPlus(
        Choice(LineBreak, Comment, Keys, Request, GenericXMLElement, ConsumeUntil(ClosingTag))
    );
    private static IParser Spaces = AnyStringP(" \t");

    private static IParser RequestsBody = new XMLElemenParser("Requests", Body);
    private static IParser Element = Seq(Optional(Spaces), RequestsBody);

    public ParseResult Parse(ParseContext context)
    {
        var res = Element.Parse(context);
        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;
        XMLElementToken xmlElement = (XMLElementToken)seq.Children[1];

        return ParseResult.SuccessAt(
            new RequestsToken() { Body = xmlElement.Body, Children = xmlElement.Children }
        );
    }
}

public class RequestsToken : IToken
{
    public Range Body { get; init; }
    public IToken[] Children { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Requests>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Requests>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
