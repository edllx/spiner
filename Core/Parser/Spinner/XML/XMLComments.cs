using System.Text;
using static spinner.Parser;

namespace spinner;

internal class XMLCommentParser : IParser
{
    private static IParser OpenningTag = StringP("<!--");
    private static IParser ClosingTag = StringP("-->");

    private static IParser Spaces = AnyStringP(" \t");
    private static IParser Service = Seq(
        Optional(Spaces),
        OpenningTag,
        TryUntil(ClosingTag, Seq(Optional(LineBreak), ConsumeUntil(Choice(LineBreak, ClosingTag)))),
        ClosingTag
    );

    public ParseResult Parse(ParseContext context)
    {
        var res = Service.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;
        var target = seq.Children[2];
        List<IToken> elem = [];
        Unroll(target, elem);

        return ParseResult.SuccessAt(new XMLCommentToken() { Children = elem.ToArray() });
    }

    private static void Unroll(IToken token, List<IToken> destination)
    {
        switch (token)
        {
            case SequenceToken seq:
                foreach (IToken t in seq.Children)
                {
                    Unroll(t, destination);
                }
                break;

            case LineBreakToken:
                break;
            default:
                destination.Add(token);
                break;
        }
    }
}

public struct XMLCommentToken(IToken[] childen) : IToken
{
    public IToken[] Children = childen;
    public Range Body { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Comment>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Comment>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
