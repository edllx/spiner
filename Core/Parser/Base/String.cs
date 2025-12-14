using static spinner.Parser;

namespace spinner;

public class StringParser(string str) : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        if (!context.HasNext())
        {
            context.Position = initialPosition;
            return ParseResult.FailAt(
                new ParseFailedToken(initialPosition, this) { At = initialPosition }
            );
        }

        for (int i = 0; context.HasNext() && i < str.Length; i++, context.Position++)
        {
            if (context.Input[context.Position] != str[i])
            {
                context.Position = initialPosition;
                return ParseResult.FailAt(new ParseFailedToken(initialPosition, this));
            }
        }

        return ParseResult.SuccessAt(
            new TextToken()
            {
                Body = new() { Start = initialPosition, Length = str.Length },
            }
        );
    }
}

public class AnyStringParser(string candidate) : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        if (!context.HasNext())
        {
            context.Position = initialPosition;
            return ParseResult.FailAt(
                new ParseFailedToken(initialPosition, this) { At = initialPosition }
            );
        }

        while (context.HasNext() && candidate.Contains(context.Input[context.Position]))
        {
            context.Position++;
        }

        if (context.Position == initialPosition)
        {
            new ParseFailedToken(initialPosition, this) { At = initialPosition };
        }

        return ParseResult.SuccessAt(
            new TextToken()
            {
                Body = new()
                {
                    Start = initialPosition,
                    Length = context.Position - initialPosition,
                },
            }
        );
    }
}

public class PrintableCharParser(string exclude) : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        if (!context.HasNext())
        {
            context.Position = initialPosition;
            return ParseResult.FailAt(
                new ParseFailedToken(initialPosition, this) { At = initialPosition }
            );
        }

        char target = context.Input[context.Position];

        while (
            context.HasNext()
            && char.IsBetween(context.Input[context.Position], ' ', '~')
            && !exclude.Contains(context.Input[context.Position])
        )
        {
            context.Position++;
        }

        if (context.Position == initialPosition)
        {
            return ParseResult.FailAt(
                new ParseFailedToken(initialPosition, this) { At = initialPosition }
            );
        }

        return ParseResult.SuccessAt(
            new TextToken()
            {
                Body = new()
                {
                    Start = initialPosition,
                    Length = context.Position - initialPosition,
                },
            }
        );
    }
}

public class AlphaCharParser : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        if (!context.HasNext())
        {
            context.Position = initialPosition;
            return ParseResult.FailAt(
                new ParseFailedToken(initialPosition, this) { At = initialPosition }
            );
        }

        char target = context.Input[context.Position];

        while (
            context.HasNext()
            && (
                (char.IsBetween(context.Input[context.Position], 'A', 'Z'))
                || char.IsBetween(context.Input[context.Position], 'a', 'z')
            )
        )
        {
            context.Position++;
        }

        return ParseResult.SuccessAt(
            new TextToken()
            {
                Body = new()
                {
                    Start = initialPosition,
                    Length = context.Position - initialPosition,
                },
            }
        );
    }
}

public class StringLiteralParser : IParser
{
    private static IParser Element = Seq(Char('"'), PrintableChar("\""), Char('"'));

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        var res = Element.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;

        return ParseResult.SuccessAt(
            new StringLiteralToken() { Body = seq.Body, Value = seq.Children[1].Body }
        );
    }
}

public class StringLiteralNestedParser : IParser
{
    private static IParser Element = Seq(Char('\''), PrintableChar("'"), Char('\''));

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        var res = Element.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;

        return ParseResult.SuccessAt(
            new StringLiteralToken() { Body = seq.Body, Value = seq.Children[1].Body }
        );
    }
}

public class StringLiteralToken : IToken
{
    public Range Body { get; init; }
    public Range Value { get; init; }

    public string ToString(string source, int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<String value=\"{Value.ToString(source)}\">\n";
    }
}
