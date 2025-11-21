using System.Text;
using static spinner.Parser;

namespace spinner;

public class ServiceLayerRunParser : IParser
{
    private static IParser Run = new XMLSingleLineElementParser("Run");
    private static IParser Spaces = AnyStringP(" \t");
    private static IParser ClosingTag = Seq(StringP("</"), StringP("Run"), Char('>'));
    private static IParser Body = ZeroPlus(Choice(LineBreak, ConsumeUntil(ClosingTag)));
    private static IParser RunBody = new XMLElemenParser("Run", Body);

    private static IParser Element = Seq(Optional(Spaces), Choice(Run, RunBody));

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

        Range command = new();
        for (int i = 0; i < xmlElement.Attributes.Length; i++)
        {
            var att = xmlElement.Attributes[i];
            switch (context.Input.AsSpan().Slice(att.Name.Start, att.Name.Length).ToString())
            {
                case "command":
                    command = att.Value;
                    break;

                default:
                    break;
            }
        }

        return ParseResult.SuccessAt(
            new ServiceLayerRunToken()
            {
                Body = seq.Body,
                Command = command,
                Children = xmlElement.Children,
            }
        );
    }
}

public class ServiceLayerRunToken : IToken
{
    public Range Body { get; init; }
    public Range Command { get; init; }
    public IToken[] Children { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        if (Command.Length > 0)
        {
            var command = $"command=\"{source.AsSpan().Slice(Command.Start, Command.Length)}\"";
            return $"{"".PadRight(4 * depth)}<Run {command} />";
        }

        var buffer = new StringBuilder();
        var lfMark = $"{"".PadRight(4 * depth)}<Run>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Run>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
