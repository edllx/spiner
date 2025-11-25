using static spinner.Parser;

namespace spinner;

public class JsonOperatorParser : IParser
{
    private static IParser Spaces = AnyStringP(" \t");
    private static IParser MemberAccess = Seq(
        Seq(Char('['), Optional(Spaces)),
        NestedStringLiteral,
        Seq(Optional(Spaces), Char(']'))
    );
    private static IParser ArrayIndex = Seq(
        Seq(Char('['), Optional(Spaces)),
        OnePlus(Digit),
        Seq(Optional(Spaces), Char(']'))
    );
    private static IParser MetadataAccess = Seq(Char('#'), AlphaChar);
    private static IParser Element = Choice(MemberAccess, ArrayIndex, MetadataAccess);

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        var res = Element.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        ChoiceToken choise = (ChoiceToken)res.Token;
        var seq = (SequenceToken)choise.Token;

        Range value = new();
        JsonOperatorType type = JsonOperatorType.MemberAccess;

        switch (choise.SelectedIndex)
        {
            case 0:
                if (seq.Children[1] is StringLiteralToken t)
                {
                    value = t.Value;
                }
                break;

            case 1:

                value = seq.Children[1].Body;
                type = JsonOperatorType.ArrayIndex;
                break;

            case 2:
                value = seq.Children[1].Body;
                type = JsonOperatorType.MetadataAccess;
                break;
        }

        var op = new JsonOperatorToken()
        {
            Body = choise.Body,
            Value = value,
            Type = type,
        };

        return ParseResult.SuccessAt(op);
    }
}

public enum JsonOperatorType
{
    MemberAccess,
    ArrayIndex,
    MetadataAccess,
}

public class JsonOperatorToken : IToken
{
    public Range Body { get; init; }
    public Range Value { get; init; }
    public JsonOperatorType Type { get; init; }

    public string ToString(string source, int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<{Type.ToString()} value=\"{Value.ToString(source)}\">\n";
    }
}
