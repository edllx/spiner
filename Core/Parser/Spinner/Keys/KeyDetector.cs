using static spinner.Parser;

namespace spinner;

internal class KeyDetector : IParser
{
    private static IParser OpenningMark = Seq(Char('{'), Char('{'));
    private static IParser ClossingMark = Seq(Char('}'), Char('}'));
    private IParser Key = Seq(OpenningMark, ConsumeUntil(ClossingMark), ClossingMark);

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        var res = Key.Parse(context);
        if (!res.Success)
        {
            return ParseResult.FailAt(
                new ParseFailedToken(initialPosition, Key) { At = initialPosition }
            );
        }

        SequenceToken seq = (SequenceToken)res.Token;
        List<IToken> tk = [];
        KeyParser.Unroll(seq.Children[1], tk);

        return ParseResult.SuccessAt(
            new KeyRefToken()
            {
                Body = res.Token.Body,
                Name = new Range() { Start = tk[0].Body.Start, Length = tk[0].Body.Length },
            }
        );
    }
}
