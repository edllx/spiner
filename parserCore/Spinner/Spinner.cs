using System.Text;
using static spinner.Parser;

namespace spinner;

public class SpinnerParser
{
    private static IParser ClosingTag = Seq(StringP("</"), StringP("Spinner"), Char('>'));
    private static IParser Comment = new XMLCommentParser();
    private static IParser Services = new SpinnerServicesParser();
    private static IParser GenericXMLElement = new XMLElemenParser(AlphaChar);
    private static IParser SpinnerBody = ZeroPlus(
        Choice(LineBreak, Comment, Services, GenericXMLElement, ConsumeUntil(ClosingTag))
    );
    private static IParser SpinnerDoc = new XMLElemenParser("Spinner", SpinnerBody);

    public ParseResult Parse(string source)
    {
        var context = new ParseContext(source);

        try { }
        catch (MissingKeyNameException ex)
        {
            Console.WriteLine(ex.Message);
        }

        var res = SpinnerDoc.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        XMLElementToken xmlElement = (XMLElementToken)res.Token;

        return ParseResult.SuccessAt(
            new SpinnerToken() { Body = xmlElement.Body, Children = xmlElement.Children }
        );
    }
}

public class SpinnerToken : IToken
{
    public IToken[] Children { get; init; } = [];
    public Range Body { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Spinner>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Spinner>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
