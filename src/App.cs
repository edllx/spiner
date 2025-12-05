namespace spinner;

public interface Iresovable
{
    void Resolve(Scope? scope = null);
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
                        GenerateComponent<List<ServiceTemplate>>(tk, source)
                    );
                    break;

                case "Requests":
                    RequestsManager.SetTemplates(
                        GenerateComponent<List<RequestTemplate>>(tk, source)
                    );
                    break;

                case "TestSuite":

                    var ts = GenerateComponent<TestSuite>(tk, source);
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
        return ToString(0);
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

public partial class App
{
    public T? GenerateComponent<T>(SpinnerToken token, string source)
        where T : new()
    {
        string mode = "";
        string name = "";
        string value = "";
        string len = "";
        string layer = "";
        string image = "";
        string buildPath = "";
        string from = "";
        string path = "";
        string method = "";

        try
        {
            switch (typeof(T))
            {
                case Type t when t == typeof(List<RequestTemplate>):
                    if (token.Name != "Requests")
                    {
                        return default(T);
                    }
                    List<RequestTemplate> requestTemplates = [];
                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        if (token.Children[i] is not SpinnerToken st)
                        {
                            continue;
                        }

                        var s = GenerateComponent<RequestTemplate>(st, source);
                        if (s is null)
                        {
                            continue;
                        }
                        requestTemplates.Add(s);
                    }

                    return (T)(object)requestTemplates;

                case Type t when t == typeof(RequestTemplate):
                    if (token.Name != "Request")
                    {
                        return default(T);
                    }
                    name = token.GetAttribute("name", source) ?? "";
                    if (string.IsNullOrEmpty(name))
                    {
                        return default(T);
                    }

                    RequestBody? b = null;

                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        if (token.Children[i] is not SpinnerToken tk)
                        {
                            continue;
                        }

                        if (tk.Name == "Body")
                        {
                            b = GenerateComponent<RequestBody>(tk, source);
                            break;
                        }
                    }

                    List<Key> requestTempatekeys =
                        GenerateComponent<List<Key>>(token, source) ?? [];
                    path = token.GetAttribute("path", source) ?? "";
                    method = token.GetAttribute("method", source) ?? "";

                    return (T)
                        (object)
                            new RequestTemplate(
                                name: name,
                                method: method,
                                scope: new(requestTempatekeys),
                                path: path,
                                body: b
                            );

                case Type t when t == typeof(RequestBody):
                    if (token.Name != "Body")
                    {
                        return default(T);
                    }

                    var type = token.GetAttribute("type", source);
                    List<Key> keys = GenerateComponent<List<Key>>(token, source) ?? [];
                    return (T)(object)new RequestBody(type: type, keys: keys.ToArray());

                case Type t when t == typeof(List<ServiceTemplate>):
                    if (token.Name != "Services")
                    {
                        return default(T);
                    }

                    List<ServiceTemplate> serviceTemplates = [];
                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        if (token.Children[i] is not SpinnerToken st)
                        {
                            continue;
                        }
                        var s = GenerateComponent<ServiceTemplate>(st, source);
                        if (s is null)
                        {
                            continue;
                        }
                        serviceTemplates.Add(s);
                    }

                    return (T)(object)serviceTemplates;

                case Type t when t == typeof(ServiceTemplate):
                    if (token.Name != "Service")
                    {
                        return default(T);
                    }

                    name = token.GetAttribute("name", source) ?? "";
                    if (string.IsNullOrEmpty(name))
                    {
                        return default(T);
                    }

                    image = token.GetAttribute("image", source) ?? "";
                    buildPath = token.GetAttribute("build", source) ?? "";

                    List<Layer> layers = [];
                    List<Key> serviceTemplateKeys =
                        GenerateComponent<List<Key>>(token, source) ?? [];

                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        if (token.Children[i] is not SpinnerToken stk)
                        {
                            continue;
                        }

                        var l = GenerateComponent<Layer>(stk, source);
                        if (l is null)
                        {
                            continue;
                        }
                        layers.Add(l);
                    }

                    return (T)
                        (object)
                            new ServiceTemplate(
                                name,
                                image: image,
                                buildPath: buildPath,
                                layers: Layer.ResolveLayer(layers.ToArray()),
                                scope: new(serviceTemplateKeys.ToArray())
                            );

                case Type t when t == typeof(Layer):
                    if (token.Name != "Layer")
                    {
                        return default(T);
                    }
                    name = token.GetAttribute("name", source) ?? "";
                    if (string.IsNullOrEmpty(name))
                    {
                        return default(T);
                    }

                    from = token.GetAttribute("from", source) ?? "";
                    List<IRun> layerCommands = [];

                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        if (token.Children[i] is not SpinnerToken stk)
                        {
                            continue;
                        }
                        var src = "";
                        var dest = "";
                        switch (stk.Name)
                        {
                            case "Copy":

                                src = stk.GetAttribute("source", source) ?? "";
                                dest = stk.GetAttribute("dest", source) ?? "";

                                if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(dest))
                                {
                                    continue;
                                }

                                layerCommands.Add(new Copy(src, dest));
                                break;
                            case "Run":
                                var cmd = stk.GetAttribute("command", source) ?? "";

                                if (!string.IsNullOrEmpty(cmd))
                                {
                                    layerCommands.Add(new Run(cmd));
                                    break;
                                }

                                if (stk.Children.Length == 0)
                                {
                                    break;
                                }

                                layerCommands.Add(
                                    new Run(
                                        string.Join(
                                            "",
                                            stk.Children.Select(v =>
                                            {
                                                if (v is XMLTextToken tx)
                                                {
                                                    return string.Join(
                                                        " ",
                                                        tx.Lines.Select(x =>
                                                            x.Body.ToString(source)
                                                        )
                                                    );
                                                }
                                                return "";
                                            })
                                        )
                                    )
                                );

                                break;
                            case "Sql":
                                src = stk.GetAttribute("source", source);
                                if (src is null)
                                {
                                    return default(T);
                                }
                                layerCommands.Add(new Copy(src, "/scripts"));
                                var filename = src.Split("/").Last().ToString();
                                // TODO Support other sql dialect
                                layerCommands.Add(
                                    new Run(
                                        "psql -U {{POSTGRES_USER}} " + $"-f /scripts/{filename}"
                                    )
                                );
                                break;
                        }
                    }
                    return (T)
                        (object)new Layer(name, from: from, commands: layerCommands.ToArray());

                case Type t when t == typeof(TestSuite):
                    if (token.Name != "TestSuite")
                    {
                        return default(T);
                    }

                    List<Tests> testSet = [];
                    Stack? stack = null;
                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        if (token.Children[i] is not SpinnerToken tk)
                        {
                            continue;
                        }

                        switch (tk.Name)
                        {
                            case "Stack":
                                stack = GenerateComponent<Stack>(tk, source);
                                break;

                            case "Tests":
                                var tests = GenerateComponent<Tests>(tk, source);
                                if (tests is null)
                                {
                                    break;
                                }
                                testSet.Add(tests);
                                break;
                            default:
                                break;
                        }
                    }
                    if (stack is null)
                    {
                        return default(T);
                    }

                    return (T)(object)new TestSuite(tests: testSet.ToArray(), testStack: stack);

                case Type t when t == typeof(Stack):
                    if (token.Name != "Stack")
                    {
                        return default(T);
                    }
                    List<Service> lServ = [];

                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        if (token.Children[i] is not SpinnerToken tk)
                        {
                            continue;
                        }

                        var s = GenerateComponent<Service>(tk, source);
                        if (s is null)
                        {
                            continue;
                        }
                        lServ.Add(s);
                    }
                    return (T)(object)new Stack(lServ.ToArray());

                case Type t when t == typeof(Service):
                    if (token.Name != "Service")
                    {
                        return default(T);
                    }
                    name = token.GetAttribute("name", source) ?? "";
                    layer = token.GetAttribute("layer", source) ?? "";
                    if (string.IsNullOrEmpty(name))
                    {
                        return default(T);
                    }

                    var serviceTemplate = ServiceManager.GetTemplate(name);
                    if (serviceTemplate is null)
                    {
                        return default(T);
                    }

                    List<IRun> serviceCommands = [];
                    if (!string.IsNullOrEmpty(layer))
                    {
                        var ll = layer.Split(",");
                        for (int i = 0; i < ll.Length; i++)
                        {
                            var l = serviceTemplate.Layers.FirstOrDefault(v => v.Name == ll[i]);
                            if (l is null)
                            {
                                continue;
                            }
                            for (int j = 0; j < l.Commands.Length; j++)
                            {
                                serviceCommands.Add(l.Commands[j].Copy());
                            }
                        }
                    }

                    return (T)
                        (object)
                            new Service(
                                serviceTemplate.Name,
                                "testId",
                                image: serviceTemplate.Image,
                                buildPath: serviceTemplate.BuildPath,
                                scope: serviceTemplate.Scope.Copy(),
                                commands: serviceCommands.ToArray()
                            );

                case Type t when t == typeof(Tests):
                    if (token.Name != "Tests")
                    {
                        return default(T);
                    }
                    token.ValidateName("Tests");
                    mode = token.GetAttribute("mode", source) ?? Tests.DefaultMode;

                    List<Key> testsKeys = GenerateComponent<List<Key>>(token, source) ?? [];
                    List<Test> testsTests = GenerateComponent<List<Test>>(token, source) ?? [];
                    Scope testsScope = new(testsKeys);
                    for (int i = 0; i < testsTests.Count; i++)
                    {
                        testsTests[i].Scope.Parent = testsScope;
                    }

                    return (T)
                        (object)
                            new Tests(testSet: testsTests.ToArray(), mode: mode, scope: testsScope);

                case Type t when t == typeof(List<Key>):

                    List<Key> lk = [];
                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        var child = token.Children[i];
                        if (token.Children[i] is not SpinnerToken stk)
                        {
                            continue;
                        }

                        var key = GenerateComponent<Key>(stk, source);
                        if (key is null)
                        {
                            continue;
                        }
                        lk.Add(key);
                    }

                    return (T)(object)lk;

                case Type t when t == typeof(List<Test>):
                    if (token.Name != "Tests")
                    {
                        return default(T);
                    }
                    List<Test> ltest = [];
                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        var child = token.Children[i];
                        if (token.Children[i] is not SpinnerToken stk)
                        {
                            continue;
                        }

                        var key = GenerateComponent<Test>(stk, source);
                        if (key is null)
                        {
                            continue;
                        }
                        ltest.Add(key);
                    }

                    return (T)(object)ltest;

                case Type t when t == typeof(Key):

                    name = token.GetAttribute("name", source) ?? "";
                    value = token.GetAttribute("value", source) ?? "";
                    len = token.GetAttribute("len", source) ?? "";

                    if (name is null)
                    {
                        return default(T);
                    }

                    switch (token.Name)
                    {
                        case "Arg":
                            return (T)(object)new Key(name, value);
                        case "Key":
                            return (T)(object)new Key(name, value);

                        case "GeneratedKey":
                            if (!int.TryParse(len, out int ln))
                            {
                                throw new Exception("Invalid generated key len");
                            }
                            value = Tools.GenerateRandomString(ln);
                            return (T)
                                (object)
                                    new Key(name, "{{Generated}}")
                                    {
                                        Generated = true,
                                        GenInfo = new() { Len = ln },
                                    };
                    }
                    return default(T);

                case Type t when t == typeof(Test):
                    if (token.Name != "Test")
                    {
                        return default(T);
                    }
                    List<Key> testKeys = GenerateComponent<List<Key>>(token, source) ?? [];
                    Scope testScope = new(testKeys);

                    TestRequest? testRequest = null;
                    TestAssert? testAssert = null;
                    TestResponse? testResponse = null;

                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        if (token.Children[i] is not SpinnerToken stk)
                        {
                            continue;
                        }

                        switch (stk.Name)
                        {
                            case "Request":
                                testRequest = GenerateComponent<TestRequest>(stk, source);
                                break;

                            case "Asserts":
                                testAssert = GenerateComponent<TestAssert>(stk, source);
                                break;

                            case "Response":
                                testResponse = GenerateComponent<TestResponse>(stk, source);
                                break;
                        }
                    }

                    return (T)
                        (object)
                            new Test(
                                request: testRequest,
                                asserts: testAssert,
                                response: testResponse,
                                scope: testScope
                            );

                case Type t when t == typeof(TestRequest):
                    if (token.Name != "Request")
                    {
                        return default(T);
                    }
                    name = token.GetAttribute("name", source) ?? "";
                    var testRequestTemplate = RequestsManager.GetTemplate(name);
                    if (testRequestTemplate is null)
                    {
                        return default(T);
                    }

                    var testRequestScope = testRequestTemplate.Scope.Copy();
                    var testRequestBody = testRequestTemplate.Body.Copy();

                    // Set body values using request Args
                    List<Key> testRequestkeys = GenerateComponent<List<Key>>(token, source) ?? [];

                    for (int i = 0; i < testRequestkeys.Count; i++)
                    {
                        var k = testRequestBody.Keys.FirstOrDefault(v =>
                            v.Name == testRequestkeys[i].Name
                        );

                        if (k is null)
                        {
                            continue;
                        }
                        k.Value = testRequestkeys[i].Value;
                    }

                    //testRequestBody.Resolve(testRequestScope);

                    return (T)
                        (object)
                            new TestRequest(
                                path: testRequestTemplate.Path,
                                method: testRequestTemplate.Method,
                                scope: testRequestScope,
                                body: testRequestBody
                            );

                case Type t when t == typeof(TestAssert):
                    if (token.Name != "Asserts")
                    {
                        return default(T);
                    }
                    List<ITestAssert> asserts = [];

                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        if (token.Children[i] is not SpinnerToken stk)
                        {
                            continue;
                        }

                        switch (stk.Name)
                        {
                            case "Equals":

                                var eq = GenerateComponent<AssertEquals>(stk, source);
                                if (eq is null)
                                {
                                    return default(T);
                                }
                                asserts.Add(eq);
                                break;
                        }
                    }
                    return (T)(object)new TestAssert(asserts.ToArray());

                case Type t when t == typeof(TestResponse):
                    if (token.Name != "Response")
                    {
                        return default(T);
                    }
                    List<Setter> setters = [];

                    for (int i = 0; i < token.Children.Length; i++)
                    {
                        if (token.Children[i] is not SpinnerToken tk)
                        {
                            continue;
                        }

                        switch (tk.Name)
                        {
                            case "Set":
                                var v = GenerateComponent<Setter>(tk, source);
                                if (v is null)
                                {
                                    return default(T);
                                }
                                setters.Add(v);
                                break;
                        }
                    }

                    return (T)(object)new TestResponse(setters.ToArray());

                case Type t when t == typeof(Setter):
                    if (token.Name != "Set")
                    {
                        return default(T);
                    }
                    var setterKey = token.GetAttribute("key", source) ?? "";
                    var setterValue = token.GetAttribute("value", source) ?? "";
                    return (T)(object)new Setter(setterKey, setterValue);

                case Type t when t == typeof(AssertEquals):
                    if (token.Name != "Equals")
                    {
                        return default(T);
                    }
                    var assetEqActual = token.GetAttribute("actual", source) ?? "";
                    var assertEqExpected = token.GetAttribute("expected", source) ?? "";
                    return (T)(object)new AssertEquals(assertEqExpected, assetEqActual);

                default:
                    return default(T);
            }
        }
        catch (Exception)
        {
            return default(T);
        }
    }
}
