using System.Text;
using static spinner.Parser;

namespace spinner;

internal class SpinnerKeyParser : IParser
{
    private static IParser Key = new XMLSingleLineTagParser("Key");
    private static IParser GeneratedKey = new XMLSingleLineTagParser("GeneratedKey");
    private static IParser Param = new XMLSingleLineTagParser("Key");

    private static IParser Service = Choice(Key, GeneratedKey, Param);

    public ParseResult Parse(ParseContext context)
    {
        var res = Service.Parse(context);
        SequenceToken seq = (SequenceToken)res.Token;
        return ParseResult.SuccessAt(new SpinnerToken() { Children = [seq.Children[1]] });
    }
}

public struct SpinnerKeyToken(IToken[] childen) : IToken
{
    public IToken[] Children = childen;
    public Range Body { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Key>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Key>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
