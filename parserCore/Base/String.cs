namespace spinner;

public class StringParser(string str) : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

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

        while (context.HasNext() && candidate.Contains(context.Input[context.Position]))
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

public class PrintableCharParser(string exclude) : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        char target = context.Input[context.Position];

        while (
            context.HasNext()
            && char.IsBetween(context.Input[context.Position], ' ', '~')
            && !exclude.Contains(context.Input[context.Position])
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

public class AlphaCharParser : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

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
