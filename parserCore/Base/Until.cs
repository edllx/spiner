namespace spiner;

public class ConsumeUntilParser(IParser end, ParserHint[] hints) : IParser
{
    public ParseResult Parse(ParseContext context)
    {

        int initialPosition = context.Position;
        List<IToken> values = [];
        var isEndParser = Parser.PositiveLookAhead(end).Parse(context);
        bool consumed = false;


        if (!context.HasNext() || isEndParser.Success)
        {
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, this) { At = initialPosition });
        }


        while (context.HasNext() && !consumed && !isEndParser.Success)
        {
            for (int i = 0; i < hints.Length; i++)
            {
                ParserHint h = hints[i];
                if (!Parser.PositiveLookAhead(h.Hint).Parse(context).Success) { continue; }
                int prevPosiion = context.Position;

                var inline = h.Candidate.Parse(context);

                if (!inline.Success)
                {
                    context.Position = prevPosiion;
                    continue;
                }

                if (prevPosiion > initialPosition)
                {
                    values.Add(new TextToken() { Body = new() { Start = initialPosition, Length = prevPosiion - initialPosition } });
                }

                values.Add(inline.Token);

                i = hints.Length;
                consumed = true;

                return ParseResult.SuccessAt(new SequenceToken(values.ToArray()) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } });

            }

            if (!consumed)
            {
                context.Position++;
                isEndParser = Parser.PositiveLookAhead(end).Parse(context);
            }
        }

        values.Add(new TextToken() { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } });

        var x = new SequenceToken(values.ToArray()) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } };
        return ParseResult.SuccessAt(new SequenceToken(values.ToArray()) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } });
    }
}


public class TryUntilParser(IParser end, IParser candidate) : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        List<IToken> values = [];
        var isEndParser = Parser.PositiveLookAhead(end).Parse(context);

        if (!context.HasNext() || isEndParser.Success)
        {
            context.Position = initialPosition;
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, this) { At = initialPosition });
        }

        var result = candidate.Parse(context);

        while (context.HasNext() && result.Success && !isEndParser.Success)
        {
            values.Add(result.Token);
            isEndParser = isEndParser = Parser.PositiveLookAhead(end).Parse(context);
            if (isEndParser.Success) { break; }
            result = candidate.Parse(context);
        }

        if (result.Success && !context.HasNext()) { values.Add(result.Token); }

        return ParseResult.SuccessAt(new SequenceToken(values.ToArray()) { Body = new() { Start = initialPosition, Length = context.Position - initialPosition } });
    }
}

public struct TextToken() : IToken
{
    public Range Body { get; init; }

    public string ToString(string source, int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<Text Content='{source.AsSpan().Slice(Body.Start, Body.Length)}'/>";
    }
}
