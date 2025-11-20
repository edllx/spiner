using System.Text;
using static spinner.Parser;

namespace spinner;

internal class XMLElemenParser : IParser
{
    private IParser OpenningTag;
    private IParser ClosingTag;
    private IParser Body;
    private IParser TagWithBody;
    private static IParser Spaces = AnyStringP(" \t");
    private IParser Tag;

    public XMLElemenParser(string str)
    {
        IParser strP = StringP(str);
        IParser attributes = new XMLAttributeParser();
        OpenningTag = Seq(Char('<'), Seq(strP, attributes, Optional(Spaces)), Char('>'));
        ClosingTag = Seq(StringP("</"), strP, Char('>'));
        Body = ConsumeUntil(ClosingTag);
        TagWithBody = Seq(OpenningTag, Body, ClosingTag);
        Tag = TagWithBody;
    }

    public XMLElemenParser(string str, IParser body)
    {
        IParser strP = StringP(str);
        IParser attributes = new XMLAttributeParser();
        OpenningTag = Seq(Char('<'), Seq(strP, attributes, Optional(Spaces)), Char('>'));
        ClosingTag = Seq(StringP("</"), strP, Char('>'));
        Body = body;
        TagWithBody = Seq(OpenningTag, Body, ClosingTag);
        Tag = TagWithBody;
    }

    public XMLElemenParser(IParser marker)
    {
        IParser attributes = new XMLAttributeParser();
        OpenningTag = Seq(Char('<'), Seq(marker, attributes, Optional(Spaces)), Char('>'));
        ClosingTag = Seq(StringP("</"), marker, Char('>'));

        Body = ConsumeUntil(ClosingTag);
        TagWithBody = Seq(OpenningTag, Body, ClosingTag);
        Tag = TagWithBody;
    }

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        var res = Tag.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        List<IToken> children = [];
        List<XMLAttributeToken> attributes = [];
        List<TextToken> texts = [];
        List<XMLCommentToken> comments = [];
        Unroll(res.Token, children, attributes, comments, texts);

        if (IsElement(context, texts[1], "Run"))
        {
            var tx = texts[4];
            var str = context.Input.AsSpan().Slice(tx.Body.Start, tx.Body.Length).ToString();
            TextToken.Normalize(str, tx.Body.Start, children);
        }

        var token = new XMLElementToken()
        {
            Children = children.ToArray(),
            Name = texts[1].Body,
            Attributes = attributes.ToArray(),
            Comments = comments.ToArray(),
        };

        return ParseResult.SuccessAt(token);
    }

    private static bool IsElement(ParseContext ctx, IToken token, string element)
    {
        return ctx.Input.AsSpan().Slice(token.Body.Start, token.Body.Length).ToString() == element;
    }

    private static void Unroll(
        IToken token,
        List<IToken> children,
        List<XMLAttributeToken> attributes,
        List<XMLCommentToken> comments,
        List<TextToken> texts
    )
    {
        switch (token)
        {
            case XMLAttributesToken tk:
                for (int i = 0; i < tk.Tokens.Length; i++)
                {
                    attributes.Add(tk.Tokens[i]);
                    children.Add(tk.Tokens[i]);
                }

                break;

            case TextToken tk:

                texts.Add(tk);
                break;

            case XMLCommentToken tk:
                comments.Add(tk);
                children.Add(tk);
                break;

            case SequenceToken seq:
                foreach (IToken t in seq.Children)
                {
                    Unroll(t, children, attributes, comments, texts);
                }
                break;

            case LineBreakToken:
            case DefaultToken:
                break;

            default:
                children.Add(token);
                break;
        }
    }
}

public class XMLElementToken : IToken
{
    public Range Body { get; init; }
    public Range Name { get; init; }
    public IToken[] Children { get; init; } = [];
    public XMLAttributeToken[] Attributes { get; init; } = [];
    public XMLCommentToken[] Comments { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();
        string name = source.AsSpan().Slice(Name.Start, Name.Length).ToString();
        var lfMark = $"{"".PadRight(4 * depth)}<{name}>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</{name}>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
