using System.Text;

namespace spinner;

public struct Range
{
    public int Start { get; init; }
    public int Length { get; init; }
    public int End => Start + Length;

    public Range() { }

    public Range(int start, int length)
    {
        Start = start;
        Length = length;
    }

    public string ToString(string source)
    {
        if (source.Length <= Length)
        {
            return "";
        }
        return source.AsSpan().Slice(Start, Length).ToString();
    }

    public string ToString(ParseContext context)
    {
        if (context.Input.Length <= Length)
        {
            return "";
        }
        return context.Input.AsSpan().Slice(Start, Length).ToString();
    }

    public override string ToString()
    {
        return $"[{Start} - {End}]";
    }
}

public enum ParserType
{
    //Base
    Failed,
    Sequence,
    PositiveLookAhead,
    NegativeLookAhead,
    Choice,
    Until,
    ConsumeUntil,
    TryUntil,
    ZeroPlus,
    OnePlus,
    And,
    Or,
    Optional,
    EOF,
    CatchAll,
    LineBreak,
    Any,
    Char,
    Digit,
}

public class ParseStat
{
    public int Calls { get; set; }
    public int CacheHit { get; set; }

    public override string ToString()
    {
        return $"{nameof(Calls)}: {Calls} - {nameof(CacheHit)}: {CacheHit} ";
    }
}

public class ParserHint(IParser hint, IParser parsers)
{
    public IParser Hint { get; init; } = hint;
    public IParser Candidate { get; init; } = parsers;
}

public class ParseContext
{
    public string Input { get; init; }
    public int Position { get; set; }
    public bool UseStat { get; init; }
    private Dictionary<(int, IParser), ParseResult> Memo { get; } = [];
    public Dictionary<IParser, ParseStat> Stats { get; init; } = [];

    public ParseContext(string input) => Input = input;

    public void BackTrack(int length)
    {
        Position = Math.Max(0, Position - length);
    }

    public bool HasNext()
    {
        return Position < Input.Length;
    }

    public bool TryGetValue(int position, IParser parser, out ParseResult result)
    {
        var hit = Memo.TryGetValue((position, parser), out result);
        return hit;
    }

    public void Memorize(int position, IParser parser, ParseResult result)
    {
        Memo.Add((position, parser), result);
    }
}

public struct ParseResult(bool success)
{
    public bool Success { get; init; } = success;
    public IToken Token { get; set; } = new DefaultToken();

    public string ToString(string source, int depth = 0)
    {
        if (Token is null)
        {
            return "";
        }
        return Token.ToString(source, depth);
    }

    public string ToString(ParseContext context, int depth = 0)
    {
        if (Token is null)
        {
            return "";
        }
        return Token.ToString(context.Input, depth);
    }

    public static ParseResult FailAt(IToken token) => new ParseResult(false) { Token = token };

    public static ParseResult SuccessAt(IToken token) => new ParseResult(true) { Token = token };
}

public interface IParser
{
    ParseResult Parse(ParseContext context);
}

public static class Parser
{
    public static IParser And(this IParser first, IParser next) => new AndParser(first, next);

    public static IParser Or(this IParser first, IParser second) => new OrParser(first, second);

    public static readonly IParser EOF = new EndOfFileParser();
    public static readonly IParser LineBreak = new LineBreakParser();
    public static readonly IParser Any = new AnyParser();
    public static readonly IParser CatchAll = new CatchAllParser();
    public static readonly IParser Digit = new DigitParser();
    public static readonly IParser Space = new CharParser(' ');

    public static IParser Seq(params IParser[] parsers) => new SequenceParser(parsers);

    public static IParser Choice(params IParser[] parsers) => new ChoiceParser(parsers);

    public static IParser ConsumeUntil(IParser end, ParserHint[] hints) =>
        new ConsumeUntilParser(end, hints);

    public static IParser ConsumeUntil(IParser end) => new ConsumeUntilParser(end, []);

    public static IParser TryUntil(IParser end, IParser candidate) =>
        new TryUntilParser(end, candidate);

    public static IParser Optional(IParser parser) => new OptionalParser(parser);

    public static IParser ZeroPlus(IParser parser) => new ZeroPlusParser(parser);

    public static IParser OnePlus(IParser parser) => new OnePlusParser(parser);

    public static IParser PositiveLookAhead(IParser parser) => new PositiveLookAheadParser(parser);

    public static IParser NegativeLookAhead(IParser parser) => new PositiveLookAheadParser(parser);

    public static IParser Char(char c) => new CharParser(c);

    public static IParser AnyChar => new AnyCharParser();

    public static IParser PrintableChar(string exclude) => new PrintableCharParser(exclude);

    public static IParser AlphaChar => new AlphaCharParser();

    public static IParser StringP(string str) => new StringParser(str);

    public static IParser AnyStringP(string str) => new AnyStringParser(str);

    public static IParser StringLiteral => new StringLiteralParser();
    public static IParser NestedStringLiteral => new StringLiteralNestedParser();
}

public static class TypeExtensions
{
    public static string Print(this Dictionary<IParser, ParseStat> stat)
    {
        StringBuilder builder = new();

        foreach (var elem in stat)
        {
            if (elem.Key.GetType().Name.Length > 12)
            {
                builder.Append($"[{elem.Key.GetType().Name}]\t: {elem.Value}\n");
            }
            else
            {
                builder.Append($"[{elem.Key.GetType().Name}]\t\t: {elem.Value}\n");
            }
        }

        return builder.ToString();
    }
}

public interface IToken
{
    public Range Body { get; init; }
    string ToString(string source, int depth = 0);
}

public readonly struct ParseFailedToken(int position, IParser parser) : IToken
{
    public Range Body { get; init; }
    public readonly int Position = position;
    public readonly IParser Parser = parser;
    public int At { get; init; }

    public string ToString(string source, int depth)
    {
        if (Parser is CharParser cp)
        {
            return $"{"".PadRight(4 * depth)}{Parser.GetType().Name} failed at {At} Char={cp.C} ";
        }
        return $"{"".PadRight(4 * depth)}{Parser.GetType().Name} failed at {At}";
    }
}

public struct DefaultToken : IToken
{
    public Range Body { get; init; }

    public string ToString(string source, int depth)
    {
        return $"{"".PadRight(4 * depth)}<Default/>\n";
    }
}
