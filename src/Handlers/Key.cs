namespace spinner;

public class HandleKeys : HandleElementRequest<List<Key>>
{
    public HandleKeys(IToken token, string source)
        : base(token, source) { }
}

public class HandleKey : HandleElementRequest<Key>
{
    public HandleKey(IToken token, string source)
        : base(token, source) { }
}

public partial class App
{
    private T? HandleElement<T>(HandleKeys request)
        where T : List<Key>
    {
        if (request.Token is not SpinnerToken token)
        {
            return default(T);
        }

        List<Key> lk = [];
        for (int i = 0; i < token.Children.Length; i++)
        {
            var child = token.Children[i];
            if (token.Children[i] is not SpinnerToken stk)
            {
                continue;
            }

            var key = HandleElement<Key>(new(stk, request.Source));
            if (key is null)
            {
                continue;
            }
            lk.Add(key);
        }

        return (T)(object)lk;
    }

    private T? HandleElement<T>(HandleKey request)
        where T : Key
    {
        if (request.Token is not SpinnerToken token)
        {
            return default(T);
        }

        var name = token.GetAttribute("name", request.Source) ?? "";
        var value = token.GetAttribute("value", request.Source) ?? "";
        var len = token.GetAttribute("len", request.Source) ?? "";
        var seed = token.GetAttribute("seed", request.Source) ?? "";
        var prefix = token.GetAttribute("prefix", request.Source) ?? "";

        if (name is null)
        {
            return default(T);
        }

        switch (token.Name)
        {
            case "Key":
                return (T)(object)new Key(name, value);

            case "GeneratedKey":
                int ln = 20;
                int sd = -1;
                int.TryParse(len, out ln);
                int.TryParse(seed, out sd);

                return (T)
                    (object)
                        new Key(name, "Generated")
                        {
                            Generated = true,
                            GenInfo = new()
                            {
                                Len = ln,
                                Seed = sd,
                                Prefix = prefix,
                            },
                        };
        }
        return default(T);
    }
}
