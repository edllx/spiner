using static spinner.Parser;

namespace spinner;

internal class XMLSingleLineElementParser : IParser
{
    private IParser SingleLineTag;
    private static IParser Spaces = AnyStringP(" \t");

    public XMLSingleLineElementParser(string str)
    {
        IParser attributes = new XMLAttributeParser();
        IParser strP = StringP(str);
        var body = Seq(
            Seq(Char('<'), Optional(Spaces)),
            Seq(strP, attributes, Optional(Spaces)),
            Seq(Char('/'), Optional(Spaces), Char('>'))
        );

        SingleLineTag = Seq(Optional(Spaces), body);
    }

    public XMLSingleLineElementParser(IParser marker)
    {
        IParser attributes = new XMLAttributeParser();
        var body = Seq(
            Seq(Char('<'), Optional(Spaces)),
            Seq(marker, attributes, Optional(Spaces)),
            Seq(Char('/'), Optional(Spaces), Char('>'))
        );

        SingleLineTag = Seq(Optional(Spaces), body);
    }

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        var res = SingleLineTag.Parse(context);

        if (!res.Success)
        {
            var tok = (ParseFailedToken)res.Token;
            return res;
        }

        List<IToken> children = [];
        List<XMLAttributeToken> attributes = [];
        List<TextToken> texts = [];
        List<XMLCommentToken> comments = [];

        //Console.WriteLine(res.ToString(context.Input));
        Unroll(res.Token, children, attributes, comments, texts);

        var token = new XMLElementToken()
        {
            Name = texts[3].Body,
            Attributes = attributes.ToArray(),
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

            case LineBreakToken:
            case DefaultToken:
                break;

            default:
                children.Add(token);
                break;
        }
    }
}
