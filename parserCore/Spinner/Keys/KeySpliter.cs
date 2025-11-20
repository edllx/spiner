using static spinner.Parser;

namespace spinner;

internal class KeySpliter : IParser
{
    private static IParser OpenningMark = Seq(Char('{'), Char('{'));
    private IParser Value = ZeroPlus(
        Seq(Optional(ConsumeUntil(OpenningMark)), Optional(KeyParser.SpinnerKey))
    );

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        var res = Value.Parse(context);

        List<IToken> tk = [];
        KeyParser.Unroll(res.Token, tk);

        return ParseResult.SuccessAt(new KeysToken() { Tokens = tk.ToArray() });
    }
}
