using Spectre.Console;

namespace spinner;

public partial class App
{
    public CLICommandOutput Init()
    {
        var cmd = "";
        try
        {
            ParseResult res = Parser.Parse(new ParseContext(Args));
            if (!res.Success)
            {
                throw new Exception("Fail to parse args");
            }
            CommandToken token = (CommandToken)res.Token;
            UnwrapCommand(token);
            cmd = string.Join(" ", Path);

            if (string.IsNullOrEmpty(_inputFile))
            {
                AnsiConsole.WriteLine(CLIArgParser.Help(cmd));
                throw new Exception("Missing input file");
            }

            return Execute();
        }
        catch (ShowHelpExeption ex)
        {
            AnsiConsole.WriteLine(CLIArgParser.Help(ex.Command));
            return new(false, ex, "");
        }
        catch (System.Exception ex)
        {
            Logger.Log(ex.Message, LogLevel.Warning);
            return new(false, ex, "");
        }
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

            switch (key)
            {
                case "-f":
                case "--file":
                    SetFileName(value);
                    break;

                case "-h":
                case "--help":
                    throw new ShowHelpExeption(string.Join(" ", Path));

                case "--debug":
                    Debug = true;
                    break;

                case "--no-image-rebuild":
                    ImageRebuild = false;
                    break;
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

    private CLICommandOutput Execute()
    {
        var cmd = string.Join(" ", Path);
        switch (cmd)
        {
            case "run":
                return Build();
            default:
                Logger.Log($"Uknown command {cmd}", LogLevel.Warning);
                throw new UnknownCommandExeption(cmd);
        }
    }

    private CLICommandOutput Build()
    {
        SpinnerParser parser = new SpinnerParser();
        if (!File.Exists(_inputFile))
        {
            var message = $"Input file: {_inputFile} not found";
            Logger.Log(message, logLevel: LogLevel.Error);
            return new(false, new FileNotFoundException(), message);
        }

        string source = File.ReadAllText(_inputFile);
        var res = parser.Parse(source);

        if (!res.Success || res.Token is not SpinnerToken token)
        {
            Logger.Log(
                $"Input file parsing failed: {res.ToString(_inputFile)}",
                logLevel: LogLevel.Error
            );

            return new(
                false,
                new Exception("Parsing failed"),
                $"Input file parsing failed: {res.ToString(_inputFile)}"
            );
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
        return new(true, null, "");
    }
}
