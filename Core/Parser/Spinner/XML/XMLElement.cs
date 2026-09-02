using System.Text;
using static spinner.Parser;

namespace spinner;

public class XML
{
    private static IParser Spaces = AnyStringP(" \t\r\n");

    private static IParser GenericXMLElementSingleLine = new XMLSingleLineElementParser(AlphaChar);
    private static IParser GenericXMLElementMultiLine = new XMLElemenParser(AlphaChar);
    public static IParser GenericElement = Seq(
        Optional(Spaces),
        Choice(GenericXMLElementSingleLine, GenericXMLElementMultiLine)
    );

    public static IParser Text = new XMLText();

    public static IParser ClosingTag(string name)
    {
        return Seq(
            Seq(Char('<'), Optional(Spaces), Char('/'), Optional(Spaces)),
            StringP(name),
            Seq(Optional(Spaces), Char('>'))
        );
    }

    public static IParser ClosingTag(IParser parser)
    {
        return Seq(
            Seq(Char('<'), Optional(Spaces), Char('/'), Optional(Spaces)),
            parser,
            Seq(Optional(Spaces), Char('>'))
        );
    }
}

public class XMLText : IParser
{
    private static IParser Element = ConsumeUntil(XML.ClosingTag(AlphaChar));

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        var res = Element.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;
        TextToken text = (TextToken)seq.Children[0];

        var lines = TextToken.Normalize(text.Body.ToString(context.Input), text.Body.Start);

        return ParseResult.SuccessAt(new XMLTextToken() { Body = text.Body, Lines = lines });
    }
}

internal class XMLElemenParser : IParser
{
    private IParser OpenningTag;
    private IParser Body;
    private IParser TagWithBody;
    private static IParser Spaces = AnyStringP(" \t");
    private IParser Tag;

    public XMLElemenParser(string str)
    {
        IParser strP = StringP(str);
        IParser attributes = new XMLAttributeParser();
        OpenningTag = Seq(
            Seq(Char('<'), Optional(Spaces)),
            Seq(strP, attributes, Optional(Spaces)),
            Char('>')
        );

        IParser closingTag = XML.ClosingTag(str);

        Body = ConsumeUntil(closingTag);
        TagWithBody = Seq(OpenningTag, Body, closingTag);
        Tag = TagWithBody;
    }

    public XMLElemenParser(string str, IParser body)
    {
        IParser strP = StringP(str);
        IParser attributes = new XMLAttributeParser();
        OpenningTag = Seq(
            Seq(Char('<'), Optional(Spaces)),
            Seq(strP, attributes, Optional(Spaces)),
            Char('>')
        );

        IParser closingTag = XML.ClosingTag(str);

        Body = body;
        TagWithBody = Seq(OpenningTag, Body, closingTag);
        Tag = TagWithBody;
    }

    public XMLElemenParser(IParser marker)
    {
        IParser attributes = new XMLAttributeParser();
        OpenningTag = Seq(
            Seq(Char('<'), Optional(Spaces)),
            Seq(marker, attributes, Optional(Spaces)),
            Char('>')
        );

        var closingTag = Seq(
            Seq(Char('<'), Optional(Spaces), Char('/'), Optional(Spaces)),
            marker,
            Seq(Optional(Spaces), Char('>'))
        );

        Body = ConsumeUntil(closingTag);
        TagWithBody = Seq(OpenningTag, Body, closingTag);
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
                }
                break;

            case TextToken tk:
                texts.Add(tk);
                break;

            case XMLCommentToken tk:
                comments.Add(tk);
                break;

            case SequenceToken seq:
                foreach (IToken t in seq.Children)
                {
                    Unroll(t, children, attributes, comments, texts);
                }
                break;
            case ChoiceToken choice:
                Unroll(choice.Token, children, attributes, comments, texts);
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

public class XMLTextToken : IToken
{
    public Range Body { get; init; }
    public TextToken[] Lines { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();
        buffer.Append(string.Join('\n', Lines.Select(el => el.ToString(source, depth))));
        var body = $"{buffer}";
        return body;
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
