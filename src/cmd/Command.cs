using System.Text;
using static spinner.Parser;

namespace spinner;

public class UnknownOptionExeption(string option) : Exception($"Unknown Option [{option}]") { }

public class UnknownCommandExeption(string command) : Exception($"Unknown command [{command}]") { }

public class MissingOptionArgument(string option)
    : Exception($"Missing Option argument [{option} <path>]") { }

public class MissingCommandArgument(string command) : Exception(command) { }

public class CLICommand : IParser
{
    public string Name { get; }
    public string Description { get; }

    public CLIArg? Arg { get; }
    public CLICommand? Parent { get; private set; }
    public CLICommand[] SubCommands { get; }
    public CLIOption[] Options { get; }

    protected IParser Element { get; init; }

    private static IParser Spaces = AnyStringP(" ");

    public CLICommand(
        string name,
        string description,
        CLICommand[]? subCommands = null,
        CLIOption[]? options = null,
        CLIArg? arg = null
    )
    {
        Name = name;
        Description = description;

        SubCommands = subCommands ?? [];
        for (int i = 0; i < SubCommands.Length; i++)
        {
            SubCommands[i].Parent = this;
        }
        Options = options ?? [];
        Arg = arg;

        Element = InitParser();
    }

    private IParser InitParser()
    {
        var commandName = Parent is null ? "" : Name;
        var options = Choice(Options.Select(v => v.Element).ToArray());
        var subCommand = Optional(Choice(SubCommands.Select(v => v.Element).ToArray()));

        return Seq(StringP(Name), Spaces, Optional(options));
    }

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;

        List<CLIOptionToken> options = [];
        Range arg = new();
        CommandToken? child = null;

        try
        {
            // Parse Name

            var nameParser = AlphaChar;
            var resName = nameParser.Parse(context);

            if (!resName.Success)
            {
                return ParseResult.FailAt(new ParseFailedToken(initialPosition, nameParser));
            }

            var commandName = resName.Token.Body.ToString(context.Input);

            if (Name != commandName)
            {
                throw new UnknownCommandExeption(commandName);
            }

            //Spaces.Parse(context);
            ParseOption(context, options);

            // parse nested command
            ParseNestedCommand(context, ref child);
            ParseArg(context, ref arg);
            if (child is not null)
            {
                return ParseResult.SuccessAt(
                    new CommandToken()
                    {
                        Body = new(initialPosition, context.Position - initialPosition),
                        Name = resName.Token.Body,
                        Options = options.ToArray(),
                        Arg = arg,
                        Child = child,
                    }
                );
            }

            ParseOption(context, options);

            var cm = new CommandToken()
            {
                Body = new(initialPosition, context.Position - initialPosition),
                Name = resName.Token.Body,
                Options = options.ToArray(),
                Arg = arg,
            };

            return ParseResult.SuccessAt(cm);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void ParseNestedCommand(ParseContext context, ref CommandToken? token)
    {
        int initialPosition = context.Position;
        for (int i = 0; i < SubCommands.Length; i++)
        {
            var el = SubCommands[i];
            var res = el.Parse(context);
            if (res.Success)
            {
                var tk = (CommandToken)res.Token;

                token = tk;
            }
        }
    }

    private void ParseArg(ParseContext context, ref Range argToken)
    {
        if (Arg is null || argToken.IsSet())
        {
            return;
        }

        var res = Seq(Spaces, PrintableChar(" ")).Parse(context);

        if (!res.Success)
        {
            return;
        }

        var sequence = (SequenceToken)res.Token;

        argToken = sequence.Children[1].Body;
    }

    private void ParseOption(ParseContext context, List<CLIOptionToken> options)
    {
        var optionParser = ZeroPlus(Seq(Optional(Spaces), Choice(Options.ToArray())));
        var resOption = optionParser.Parse(context);
        int initialPosition = context.Position;

        var generic = CLIOption.GenericOption.Parse(context);
        if (generic.Success)
        {
            context.Position = initialPosition;
            throw new UnknownOptionExeption(generic.Token.Body.ToString(context.Input));
        }

        if (resOption.Success)
        {
            var sequence = (SequenceToken)resOption.Token;

            for (int i = 0; i < sequence.Children.Length; i++)
            {
                var el = (SequenceToken)sequence.Children[i];
                var optionChoice = (ChoiceToken)el.Children[1];
                var option = (CLIOptionToken)optionChoice.Token;

                options.Add(option);
            }
        }
    }

    public string Help()
    {
        var buffer = new StringBuilder();
        // Usase
        List<string> commandPath = [];

        var p = this;

        while (p is not null)
        {
            commandPath.Add(p.Name);
            p = p.Parent;
        }

        buffer.Append(
            $"Usage: {string.Join(" ", commandPath.Reverse<string>())} {(Options.Length > 0 ? "[Options]" : "")} {(Arg is not null ? $"<{Arg.Name}>" : "")} {(SubCommands.Length > 0 ? "COMMAND" : "")}\n\n"
        );
        buffer.Append($"{Description}\n\n");

        // Commands
        buffer.Append($"Commands:\n");
        for (int i = 0; i < SubCommands.Length; i++)
        {
            buffer.Append($"  {SubCommands[i].Name}\t{SubCommands[i].Description}:");
        }

        buffer.Append($"\n");

        // Options

        buffer.Append($"Options:\n");

        buffer.Append(
            string.Join(
                "\n",
                Options.Select(v =>
                {
                    return $" {(string.IsNullOrEmpty(v.Alias) ? "   " : $"-{v.Alias},")} --{v.Name}\t\t{v.Description}";
                })
            )
        );

        buffer.Append($"\n");

        return buffer.ToString();
    }
}

public class CommandToken : IToken
{
    public Range Body { get; init; }
    public Range Name { get; init; }
    public CommandToken? Child { get; init; }
    public CLIOptionToken[] Options { get; init; } = [];
    public Range Arg { get; init; }
    private bool _isLastLineBreak;

    public string ToString(string source, int depth = 0)
    {
        var arg = Arg.IsSet() ? $"Arg=\"{Arg.ToString(source)}\"" : "";
        if (Child is null && Options.Length == 0 && !Arg.IsSet())
        {
            return $"{"".PadRight(4 * depth)}<Command name=\"{Name.ToString(source)}\" {arg}/>";
        }

        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Command name=\"{Name.ToString(source)}\" {arg}>";
        if (Options.Length > 0)
        {
            AppedLineBreak(buffer);
            AppedText(
                buffer,
                string.Join('\n', Options.Select(el => el.ToString(source, depth + 1)))
            );
        }

        if (Child is not null)
        {
            AppedLineBreak(buffer);
            AppedText(buffer, Child.ToString(source, depth + 1));
        }

        var rgMark = $"\n{"".PadRight(4 * depth)}</Command>";
        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }

    private void AppedLineBreak(StringBuilder builder)
    {
        if (_isLastLineBreak)
        {
            return;
        }
        builder.Append("\n");
        _isLastLineBreak = true;
    }

    private void AppedText(StringBuilder builder, string text)
    {
        builder.Append(text);
        _isLastLineBreak = false;
        _isLastLineBreak = text.Last() == '\n';
    }
}
