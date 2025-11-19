using System.Text;
using static spinner.Parser;

namespace spinner;

internal class XMLElemenParser : IParser
{
    private IParser OpenningTag;
    private IParser ClosingTag;
    private IParser Body;
    private IParser TagWithBody;

    private IParser Tag;

    public XMLElemenParser(string str)
    {
        IParser strP = StringP(str);
        IParser attributes = new XMLAttributeParser();
        OpenningTag = Seq(Char('<'), Seq(strP, attributes), Char('>'));
        ClosingTag = Seq(StringP("</"), strP, Char('>'));
        Body = ConsumeUntil(ClosingTag);
        TagWithBody = Seq(OpenningTag, Body, ClosingTag);
        Tag = TagWithBody;
    }

    public XMLElemenParser(string str, IParser body)
    {
        IParser strP = StringP(str);
        IParser attributes = new XMLAttributeParser();
        OpenningTag = Seq(Char('<'), Seq(strP, attributes), Char('>'));
        ClosingTag = Seq(StringP("</"), strP, Char('>'));
        Body = body;
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

        //Console.WriteLine($"=>{res.ToString(context.Input)}\n");
        /*
        Console.WriteLine(
            $"--------\n{string.Join("\n", attributes.Select(v => v.ToString(context.Input)))}\n"
        );
        Console.WriteLine(
            $"{string.Join("\n", texts.Select(v => v.ToString(context.Input)))}\n--------\n"
        );
        */

        var token = new XMLElementToken()
        {
            Children = children.ToArray(),
            Name = texts[1].Body,
            Attributes = attributes.ToArray(),
            Comments = comments.ToArray(),
        };

        return ParseResult.SuccessAt(token);
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

internal class XMLSingleLineTagParser : IParser
{
    private IParser SingleLineTag;

    public XMLSingleLineTagParser(string str)
    {
        IParser strP = StringP(str);
        SingleLineTag = Seq(Char('<'), strP, StringP("/>"));
    }

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        var res = SingleLineTag.Parse(context);

        return res;
    }
}

public class XMLAttributeParser : IParser
{
    private static IParser Attribute = new XMLAttributeDetector();
    private static IParser Spaces = StringP(" ");
    private static IParser Attributes = ZeroPlus(Seq(Spaces, Attribute));

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        List<XMLAttributeToken> attributes = [];

        var res = Attributes.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        Unroll(res.Token, attributes);

        var token = new XMLAttributesToken()
        {
            Body = new() { Start = initialPosition, Length = context.Position - initialPosition },
            Tokens = attributes.ToArray(),
        };

        return ParseResult.SuccessAt(token);
    }

    private static void Unroll(IToken token, List<XMLAttributeToken> destination)
    {
        switch (token)
        {
            case XMLAttributeToken tk:
                destination.Add(tk);
                break;

            case SequenceToken seq:
                foreach (IToken t in seq.Children)
                {
                    Unroll(t, destination);
                }
                break;

            default:
                break;
        }
    }

    private class XMLAttributeDetector : IParser
    {
        private static IParser Attribute = Seq(
            AlphaChar,
            StringP("=\""),
            PrintableChar("\""),
            Char('"')
        );

        public ParseResult Parse(ParseContext context)
        {
            int initialPosition = context.Position;
            var res = Attribute.Parse(context);

            if (!res.Success)
            {
                if (!res.Success)
                {
                    var token = (ParseFailedToken)res.Token;
                    return res;
                }

                return res;
            }

            SequenceToken seq = (SequenceToken)res.Token;

            return ParseResult.SuccessAt(
                new XMLAttributeToken()
                {
                    Body = new()
                    {
                        Start = initialPosition,
                        Length = context.Position - initialPosition,
                    },
                    Name = seq.Children[0].Body,
                    Value = seq.Children[2].Body,
                }
            );
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
        throw new NotImplementedException();
    }
}

public class XMLAttributeToken : IToken
{
    public Range Body { get; init; }
    public Range Name { get; init; }
    public Range Value { get; init; }

    public string ToString(string source, int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<Attribute name=\"{source.AsSpan().Slice(Name.Start, Name.Length)}\" value=\"{source.AsSpan().Slice(Value.Start, Value.Length)}\"/>";
    }
}

public class XMLAttributesToken : IToken
{
    public Range Body { get; init; }
    public XMLAttributeToken[] Tokens { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Attributes>\n";
        buffer.Append(string.Join('\n', Tokens.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Attributes>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}
