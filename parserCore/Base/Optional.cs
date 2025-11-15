namespace spiner;

public class OptionalParser : IParser
{
    private IParser _parser;
    public OptionalParser(IParser parser) => _parser = parser;

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        ParseResult result = _parser.Parse(context);
        if (result.Success)
        {
            return ParseResult.SuccessAt(result.Token);
        }
        context.Position = initialPosition;
        return ParseResult.SuccessAt(new DefaultToken() { });
    }
}

public struct OptionalToken(IToken child) : IToken
{
    public IToken Child = child;
    public Range Body { get; init; }

    public string ToString(string source, int depth)
    {
        return Child.ToString(source, depth);
    }
}
