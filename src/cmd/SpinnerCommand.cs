using static spinner.Parser;

namespace spinner;

public class CLIArgParser : IParser
{
    private static CLIOption HelpOption = new(
        "help",
        "Get more information on the command",
        alias: "h"
    );

    private static CLIOption RunOutputFile = new(
        "output-file",
        "Redirect output to this file",
        alias: "o",
        arg: new("file", type: CLIArgType.String, required: true)
    );

    private static CLIOption RunDebug = new("debug", "Enable Debug logs");

    private static CLIOption RunNoImageRebuil = new("no-image-rebuild", "Disable image rebuil");

    private static CLIOption RunInputFile = new(
        "file",
        "File to process",
        alias: "f",
        arg: new("file", type: CLIArgType.String, required: true)
    );

    public static CLICommand Run = new(
        "run",
        "Run spinner file",
        options: [HelpOption, RunDebug, RunNoImageRebuil, RunOutputFile, RunInputFile],
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
}
