using System.Text;
using static spinner.Parser;

namespace spinner;

public enum CLIArgType
{
    String,
    Number,
    Boolean,
}

public class CLIArg
{
    public string Name { get; }
    public CLIArgType Type { get; }
    public bool Required { get; }
    public IParser Element { get; init; }

    public CLIArg(string name, bool? required = false, CLIArgType? type = CLIArgType.String)
    {
        Name = name;
        Required = required ?? false;
        Type = type ?? CLIArgType.String;
        Element = InitParser();
    }

    private IParser InitParser()
    {
        return Seq();
    }

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        var res = Element.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        var seq = (SequenceToken)res.Token;

        return ParseResult.SuccessAt(new CLIArgToken() { Body = seq.Body });
    }
}

public class CLIArgToken : IToken
{
    public Range Body { get; init; }
    public Range Value { get; init; }

    public string ToString(string source, int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<Arg Value=\"{Value.ToString(source)}\"/>";
    }
}
