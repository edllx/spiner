using static spinner.JsonParser;
using static spinner.Parser;

namespace spinner;

public class JsonResponseOperatorParser : IParser
{
    private static IParser OpenningMark = Seq(Char('{'), Char('{'));
    private static IParser ClossingMark = Seq(Char('}'), Char('}'));

    private static IParser OperatorSequence = Choice(
        Seq(StringP("response['json']"), ZeroPlus(Operator)),
        StringP("response['status']")
    );

    private static IParser Key = ConsumeUntil(ClossingMark);

    private static IParser Element = Seq(OpenningMark, Choice(OperatorSequence, Key), ClossingMark);

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        var res = Element.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;
        ChoiceToken choise = (ChoiceToken)seq.Children[1];

        if (choise.SelectedIndex == 1)
        {
            return ParseResult.SuccessAt(
                new JsonResponseOperatorToken()
                {
                    Body = seq.Body,
                    Key = choise.Body,
                    Type = JsonResponseOperatorTokenType.Key,
                }
            );
        }

        ChoiceToken opChoise = (ChoiceToken)choise.Token;

        switch (opChoise.SelectedIndex)
        {
            case 0:
                var opSeq = (SequenceToken)opChoise.Token;

                return ParseResult.SuccessAt(
                    new JsonResponseOperatorToken()
                    {
                        Body = seq.Body,
                        Key = opSeq.Children[1].Body,
                        Type = JsonResponseOperatorTokenType.Operator,
                    }
                );

            case 1:
                return ParseResult.SuccessAt(
                    new JsonResponseOperatorToken()
                    {
                        Body = seq.Body,
                        Key = opChoise.Body,
                        Type = JsonResponseOperatorTokenType.Status,
                    }
                );
        }

        return ParseResult.SuccessAt(
            new JsonResponseOperatorToken()
            {
                Body = seq.Body,
                Key = choise.Body,
                Type = JsonResponseOperatorTokenType.Key,
            }
        );
    }
}

public enum JsonResponseOperatorTokenType
{
    Key,
    Operator,
    Status,
}

public class JsonResponseOperatorToken : IToken
{
    public Range Body { get; init; }
    public Range Key { get; init; }
    public JsonResponseOperatorTokenType Type { get; init; }

    public string ToString(string source, int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<{Type} key=\"{Key.ToString(source)}\">\n";
    }
}
