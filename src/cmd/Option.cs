using static spinner.Parser;

namespace spinner;

public class CLIOption : IParser
{
    public string Name { get; init; }
    public string Description { get; init; }

    public CLIArg? Arg { get; init; }
    public string? Alias { get; init; }
    public bool Required { get; init; }
    public IParser Element { get; init; }
    private static IParser Spaces = AnyStringP(" ");
    public static IParser GenericOption { get; } =
        Seq(
            Optional(Spaces),
            Choice(Seq(Char('-'), PrintableChar(" ")), Seq(StringP("--"), PrintableChar(" ")))
        );

    public CLIOption(
        string name,
        string description,
        string? alias = null,
        bool? required = false,
        CLIArg? arg = null
    )
    {
        Name = name;
        Description = description;
        Alias = alias;
        Required = required ?? false;
        Arg = arg;

        Element = InitParser();
    }

    private IParser InitParser()
    {
        List<IParser> choiceList = [StringP($"--{Name}")];

        if (Alias is not null)
        {
            choiceList.Add(StringP($"-{Alias}"));
        }

        if (Arg is not null)
        {
            return Choice(
                Seq(Choice(choiceList.ToArray()), Optional(Seq(Char(' '), PrintableChar(" "))))
            );
        }
        return Choice(Seq(Choice(choiceList.ToArray())));
    }

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        var res = Element.Parse(context);

        if (!res.Success)
        {
            context.Position = initialPosition;
            return res;
        }

        var choice = (ChoiceToken)res.Token;
        var sequence = (SequenceToken)choice.Token;

        if (Arg is not null && sequence.Children.Length == 1)
        {
            throw new MissingOptionArgument(sequence.Body.ToString(context.Input));
        }

        Range value = new();

        if (sequence.Children.Length == 2)
        {
            SequenceToken seq = (SequenceToken)sequence.Children[1];
            value = new(seq.Children[1].Body.Start, seq.Children[1].Body.Length);
        }

        return ParseResult.SuccessAt(
            new CLIOptionToken()
            {
                Required = Required,
                Body = choice.Body,
                Key = sequence.Children[0].Body,
                Value = value,
            }
        );
    }
}

public class CLIOptionToken : IToken
{
    public Range Body { get; init; }
    public Range Key { get; init; }
    public Range Value { get; init; }
    public bool Required { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var value = $"{(Value.IsSet() ? $"Value=\"{Value.ToString(source)}\"" : "")}";
        return $"{"".PadRight(4 * depth)}<Option Key=\"{Key.ToString(source)}\" {value}/>";
    }
}
