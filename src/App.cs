namespace spinner;

public abstract class HandleElementRequest<T>
{
    public HandleElementRequest(IToken token, string source)
    {
        Token = token;
        this.Source = source;
    }

    public IToken Token { get; init; }
    public string Source { get; init; }
}

public partial class App
{
    public string Args { get; } = "";
    private CLIArgParser Parser = new();
    private string _inputFile = "";
    private List<(string, string)> Options = [];
    private List<string> Path = [];
    public string ErrorMessage = "";

    public ServiceManager ServiceManager { get; set; } = new();
    public RequestManager RequestsManager { get; set; } = new();
    public TestsManager TestManager { get; set; } = new();

    public App(string args)
    {
        Args = args;
    }

    public App() { }

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

        Execute();

        return true;
    }

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
                Build();
                break;
            default:

                break;
        }
    }

    private void Build()
    {
        SpinnerParser parser = new SpinnerParser();
        if (!File.Exists(_inputFile))
        {
            return;
        }
        string source = File.ReadAllText(_inputFile);
        var res = parser.Parse(source);

        if (!res.Success || res.Token is not SpinnerToken token)
        {
            Console.WriteLine(res.ToString(source));
            return;
        }

        List<TestSuite> testSuites = [];

        for (int i = 0; i < token.Children.Length; i++)
        {
            if (token.Children[i] is not SpinnerToken tk)
            {
                continue;
            }

            switch (tk.Name)
            {
                case "Services":
                    ServiceManager.SetTemplates(
                        HandleElement<List<ServiceTemplate>>(new(tk, source))
                    );
                    break;

                case "Requests":
                    RequestsManager.SetTemplates(
                        HandleElement<List<RequestTemplate>>(new(tk, source))
                    );
                    break;
            }
        }

        for (int i = 0; i < token.Children.Length; i++)
        {
            if (token.Children[i] is not SpinnerToken tk)
            {
                continue;
            }

            switch (tk.Name)
            {
                case "TestSuite":

                    var ts = HandleElement<TestSuite>(new(tk, source));
                    if (ts is not null)
                    {
                        testSuites.Add(ts);
                    }
                    break;
            }
        }

        TestManager.SetTemplates(testSuites);
    }

    public override string ToString()
    {
        return "";
    }

    public string ToString(int depth)
    {
        return string.Join(
            "\n\n",
            [
                ServiceManager.ToString(depth),
                RequestsManager.ToString(depth),
                TestManager.ToString(depth),
            ]
        );
    }
}
