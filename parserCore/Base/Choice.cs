namespace spinner;

public class ChoiceParser(params IParser[] parsers) : IParser
{
    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        for (int i = 0; i < parsers.Length; i++)
        {
            var result = parsers[i].Parse(context);
            if (result.Success)
            {
                var tk = new ChoiceToken()
                {
                    Body = result.Token.Body,
                    SelectedIndex = i,
                    Token = result.Token,
                    SelectedParser = parsers[i],
                };
                return ParseResult.SuccessAt(tk);
            }
        }

        context.Position = initialPosition;
        return ParseResult.FailAt(
            new ParseFailedToken(initialPosition, this) { Body = new() { Start = initialPosition } }
        );
    }
}

public class ChoiceToken : IToken
{
    public Range Body { get; init; }
    public int SelectedIndex { get; init; }
    public required IToken Token { get; init; }
    public required IParser SelectedParser { get; init; }

    public string ToString(string source, int depth = 0)
    {
        return Token.ToString(source, depth);
    }
}
