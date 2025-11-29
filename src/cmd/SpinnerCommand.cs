using static spinner.Parser;

namespace spinner;

public class CLIArgParser : IParser
{
    private static CLIOption HelpOption = new(
        "help",
        "Get more information on the command",
        alias: "h"
    );

    //private static CLIOption XCat = new("xcat", "Get more information on the command", alias: "x");

    private static CLIOption RunOutputFile = new(
        "output-file",
        "Redirect output to this file",
        alias: "o",
        arg: new("file", type: CLIArgType.String, required: true)
    );

    private static CLIOption RunInputFile = new(
        "file",
        "File to process",
        alias: "f",
        arg: new("file", type: CLIArgType.String, required: true)
    );

    /*
    private static CLICommand Dog = new("dog", "Test nested command", options: [HelpOption]);

    private static CLICommand Cat = new(
        "cat",
        "Test nested command",
        options: [HelpOption, XCat],
        subCommands: [Dog]
    );
    */

    public static CLICommand Run = new(
        "run",
        "Run spinner file",
        options: [HelpOption, RunOutputFile, RunInputFile],
        arg: new("file", type: CLIArgType.String),
        subCommands: []
    );

    private static CLICommand Spinner = new(
        "spinner",
        "API testing framework",
        options: [HelpOption],
        subCommands: [Run]
    );

    private static IParser Element = Choice(Run);

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        var res = Element.Parse(context);

        if (!res.Success)
        {
            return ParseResult.FailAt(new ParseFailedToken(initialPosition, Element));
        }

        var choice = (ChoiceToken)res.Token;

        return ParseResult.SuccessAt(choice.Token);
    }

    public string Help(CommandType command)
    {
        switch (command)
        {
            case CommandType.Run:
                return Run.Help();

            default:
                return Spinner.Help();
        }
    }
}

public enum CommandType
{
    Unknown,
    Run,
}

public class SpinnerArgToken : IToken
{
    public CommandType Command { get; init; }
    public Range Arg { get; init; }
    public CLIOptionToken[] Options { get; init; } = [];
    public Range Body { get; init; }

    public string ToString(string source, int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<{Command} Arg=\"{Arg.ToString(source)}\" Options\"{string.Join(",", Options.Select(v => v.ToString(source)))}\"/>";
    }
}
