namespace spinner;

public class App
{
    private string Args = "";
    private CLIArgParser Parser = new();
    private string _inputFile = "";
    private List<(string, string)> Options = [];
    private List<string> Path = [];
    public string ErrorMessage = "";

    public bool Init()
    {
        ParseResult res = Parser.Parse(new ParseContext(Args));

        if (!res.Success)
        {
            throw new Exception("Fail to parse args");
        }
        CommandToken token = (CommandToken)res.Token;

        UnwrapCommand(token);

        if (string.IsNullOrEmpty(_inputFile))
        {
            throw new MissingCommandArgument("input file");
        }

        return true;
    }

    private void ValidateState() { }

    private void UnwrapCommand(CommandToken token)
    {
        Path.Add(token.Name.ToString(Args));
        if (token.Arg.IsSet())
        {
            SetFileName(token.Arg.ToString(Args));
        }

        for (int i = 0; i < token.Options.Length; i++)
        {
            var el = token.Options[i];
            var key = el.Key.ToString(Args);
            var value = el.Value.ToString(Args);
            if (el.Required && string.IsNullOrEmpty(value))
            {
                throw new MissingOptionArgument(key);
            }

            if (key == "-f" || key == "--file")
            {
                SetFileName(value);
            }

            Options.Add((key, value));
        }

        if (token.Child is null)
        {
            return;
        }
        UnwrapCommand(token.Child);
    }

    private void SetFileName(string name)
    {
        if (!string.IsNullOrEmpty(_inputFile))
        {
            return;
        }
        _inputFile = name;
    }

    private void Execute()
    {
        switch (string.Join(" ", Path))
        {
            case "run":
                //Console.WriteLine($"spinner run\n - {string.Join("\n - ", Options)}");
                break;
            default:
                //Console.WriteLine($"{Parser.Help(CommandType.Unknown)}");

                break;
        }
    }

    public App()
    {
        Args = "";
    }

    public App(string args)
    {
        Args = args;
    }
}
