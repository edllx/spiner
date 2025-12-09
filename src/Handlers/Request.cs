namespace spinner;

public class HandleRequestTemplates : HandleElementRequest<List<RequestTemplate>>
{
    public HandleRequestTemplates(IToken token, string source)
        : base(token, source) { }
}

public class HandleRequestTemplate : HandleElementRequest<List<RequestTemplate>>
{
    public HandleRequestTemplate(IToken token, string source)
        : base(token, source) { }
}

public class HandleRequestBody : HandleElementRequest<RequestBody>
{
    public HandleRequestBody(IToken token, string source)
        : base(token, source) { }
}

// RequestTemplate Handlers
public partial class App
{
    private T? HandleElement<T>(HandleRequestTemplates request)
        where T : List<RequestTemplate>
    {
        if (request.Token is not SpinnerToken tk || tk.Name != "Requests")
        {
            return default(T);
        }

        List<RequestTemplate> requestTemplates = [];
        for (int i = 0; i < tk.Children.Length; i++)
        {
            if (tk.Children[i] is not SpinnerToken st)
            {
                continue;
            }

            var s = HandleElement<RequestTemplate>(new(st, request.Source));

            if (s is null)
            {
                continue;
            }
            requestTemplates.Add(s);
        }

        return (T)(object)requestTemplates;
    }

    private T? HandleElement<T>(HandleRequestTemplate request)
        where T : RequestTemplate
    {
        if (request.Token is not SpinnerToken token || token.Name != "Request")
        {
            return default(T);
        }
        var name = token.GetAttribute("name", request.Source) ?? "";
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
                b = HandleElement<RequestBody>(new(tk, request.Source));
                break;
            }
        }

        List<Key> requestTempatekeys = HandleElement<List<Key>>(new(token, request.Source)) ?? [];
        var path = token.GetAttribute("path", request.Source) ?? "";
        var method = token.GetAttribute("method", request.Source) ?? "";

        return (T)
            (object)
                new RequestTemplate(
                    name: name,
                    method: method,
                    scope: new(requestTempatekeys),
                    path: path,
                    body: b
                );
    }

    private T? HandleElement<T>(HandleRequestBody request)
        where T : RequestBody
    {
        if (request.Token is not SpinnerToken token || token.Name != "Body")
        {
            return default(T);
        }
        var type = token.GetAttribute("type", request.Source);
        List<Key> keys = HandleElement<List<Key>>(new(token, request.Source)) ?? [];
        return (T)(object)new RequestBody(type: type, keys: keys.ToArray());
    }
}
