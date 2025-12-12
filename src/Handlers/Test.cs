namespace spinner;

public class HandleTestSuite : HandleElementRequest<TestSuite>
{
    public HandleTestSuite(IToken token, string source)
        : base(token, source) { }
}

public class HandleStack : HandleElementRequest<Stack>
{
    public HandleStack(IToken token, string source)
        : base(token, source) { }
}

public class HandleTests : HandleElementRequest<Tests>
{
    public HandleTests(IToken token, string source)
        : base(token, source) { }
}

public class HandleTestSet : HandleElementRequest<List<Test>>
{
    public HandleTestSet(IToken token, string source)
        : base(token, source) { }
}

public class HandleTest : HandleElementRequest<Setter>
{
    public HandleTest(IToken token, string source)
        : base(token, source) { }
}

public class HandleArg : HandleElementRequest<Arg>
{
    public HandleArg(IToken token, string source)
        : base(token, source) { }
}

public class HandleSetter : HandleElementRequest<Setter>
{
    public HandleSetter(IToken token, string source)
        : base(token, source) { }
}

public class HandleTestRequest : HandleElementRequest<TestRequest>
{
    public HandleTestRequest(IToken token, string source)
        : base(token, source) { }
}

public class HandleTestResponse : HandleElementRequest<TestResponse>
{
    public HandleTestResponse(IToken token, string source)
        : base(token, source) { }
}

public class HandleTestAssert : HandleElementRequest<TestAssert>
{
    public HandleTestAssert(IToken token, string source)
        : base(token, source) { }
}

public class HandleTestAssertEquals : HandleElementRequest<AssertEquals>
{
    public HandleTestAssertEquals(IToken token, string source)
        : base(token, source) { }
}

public partial class App
{
    private T? HandleElement<T>(HandleTestSuite request)
        where T : TestSuite
    {
        if (request.Token is not SpinnerToken token || token.Name != "TestSuite")
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

                    stack = HandleElement<Stack>(new(tk, request.Source));
                    break;

                case "Tests":
                    var tests = HandleElement<Tests>(new(tk, request.Source));
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
    }

    private T? HandleElement<T>(HandleStack request)
        where T : Stack
    {
        if (request.Token is not SpinnerToken token || token.Name != "Stack")
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

            var s = HandleElement<Service>(new(tk, request.Source));
            if (s is null)
            {
                continue;
            }

            lServ.Add(s);
        }

        var sk = new Stack(lServ.ToArray());
        for (int i = 0; i < lServ.Count; i++)
        {
            lServ[i].Scope.Resolve();
        }
        for (int i = 0; i < lServ.Count; i++)
        {
            lServ[i].ApplyArgs(sk);
        }
        for (int i = 0; i < lServ.Count; i++)
        {
            lServ[i].ResolveLayer();
        }
        return (T)(object)sk;
    }

    private T? HandleElement<T>(HandleTests request)
        where T : Tests
    {
        if (request.Token is not SpinnerToken token || token.Name != "Tests")
        {
            return default(T);
        }

        var mode = token.GetAttribute("mode", request.Source) ?? Tests.DefaultMode;
        List<Key> testsKeys = HandleElement<List<Key>>(new(token, request.Source)) ?? [];
        List<Test> testsTests = HandleElement<List<Test>>(new(token, request.Source)) ?? [];
        Scope testsScope = new(testsKeys);
        for (int i = 0; i < testsTests.Count; i++)
        {
            testsTests[i].Scope.Parent = testsScope;
            testsTests[i].Resolve();
        }

        return (T)(object)new Tests(testSet: testsTests.ToArray(), mode: mode, scope: testsScope);
    }

    private T? HandleElement<T>(HandleTestSet request)
        where T : List<Test>
    {
        if (request.Token is not SpinnerToken token || token.Name != "Tests")
        {
            return default(T);
        }

        List<Test> ltest = [];
        for (int i = 0; i < token.Children.Length; i++)
        {
            var child = token.Children[i];
            if (token.Children[i] is not SpinnerToken tk)
            {
                continue;
            }

            var key = HandleElement<Test>(new(tk, request.Source));
            if (key is null)
            {
                continue;
            }
            ltest.Add(key);
        }

        return (T)(object)ltest;
    }

    private T? HandleElement<T>(HandleArg request)
        where T : Arg
    {
        if (request.Token is not SpinnerToken token || token.Name != "Arg")
        {
            return default(T);
        }

        var argKey = token.GetAttribute("key", request.Source) ?? "";
        var value = token.GetAttribute("value", request.Source) ?? "";
        var from = token.GetAttribute("from", request.Source) ?? "";

        if (argKey is null)
        {
            return default(T);
        }

        return (T)(object)new Arg(argKey, value, from: from);
    }

    private T? HandleElement<T>(HandleTest request)
        where T : Test
    {
        if (request.Token is not SpinnerToken token || token.Name != "Test")
        {
            return default(T);
        }

        Scope testScope = new();
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
                    testRequest = HandleElement<TestRequest>(new(stk, request.Source));
                    if (testRequest is null)
                    {
                        break;
                    }

                    testScope = testRequest.Scope;
                    break;

                case "Asserts":
                    testAssert = HandleElement<TestAssert>(new(stk, request.Source));
                    break;

                case "Response":
                    testResponse = HandleElement<TestResponse>(new(stk, request.Source));
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
    }

    private T? HandleElement<T>(HandleTestRequest request)
        where T : TestRequest
    {
        if (request.Token is not SpinnerToken token || token.Name != "Request")
        {
            return default(T);
        }

        var name = token.GetAttribute("name", request.Source) ?? "";
        var testRequestTemplate = RequestsManager.GetTemplate(name);
        if (testRequestTemplate is null)
        {
            return default(T);
        }

        var testRequestScope = testRequestTemplate.Scope.Copy();
        var testRequestBody = testRequestTemplate.Body.Copy();

        // Set body values using request Args
        List<Key> testRequestkeys = HandleElement<List<Key>>(new(token, request.Source)) ?? [];

        for (int i = 0; i < token.Children.Length; i++)
        {
            var arg = HandleElement<Arg>(new(token.Children[i], request.Source));
            if (arg is null)
            {
                continue;
            }
            testRequestScope.Set(arg.Key, arg.Value);
        }

        return (T)
            (object)
                new TestRequest(
                    name: name,
                    path: testRequestTemplate.Path,
                    method: testRequestTemplate.Method,
                    scope: testRequestScope,
                    body: testRequestBody
                );
    }

    private T? HandleElement<T>(HandleTestResponse request)
        where T : TestResponse
    {
        if (request.Token is not SpinnerToken token || token.Name != "Response")
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
                    var v = HandleElement<Setter>(new(tk, request.Source));
                    if (v is null)
                    {
                        return default(T);
                    }
                    setters.Add(v);
                    break;
            }
        }

        return (T)(object)new TestResponse(setters.ToArray());
    }

    private T? HandleElement<T>(HandleTestAssert request)
        where T : TestAssert
    {
        if (request.Token is not SpinnerToken token || token.Name != "Asserts")
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

                    var eq = HandleElement<AssertEquals>(new(stk, request.Source));
                    if (eq is null)
                    {
                        return default(T);
                    }
                    asserts.Add(eq);
                    break;
            }
        }
        return (T)(object)new TestAssert(asserts.ToArray());
    }

    private T? HandleElement<T>(HandleSetter request)
        where T : Setter
    {
        if (request.Token is not SpinnerToken token || token.Name != "Set")
        {
            return default(T);
        }

        var setterKey = token.GetAttribute("key", request.Source) ?? "";
        var setterValue = token.GetAttribute("value", request.Source) ?? "";
        return (T)(object)new Setter(setterKey, setterValue);
    }

    private T? HandleElement<T>(HandleTestAssertEquals request)
        where T : AssertEquals
    {
        if (request.Token is not SpinnerToken token || token.Name != "Equals")
        {
            return default(T);
        }

        var assetEqActual = token.GetAttribute("actual", request.Source) ?? "";
        var assertEqExpected = token.GetAttribute("expected", request.Source) ?? "";
        return (T)(object)new AssertEquals(assertEqExpected, assetEqActual);
    }
}
