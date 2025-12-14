namespace spinner;

public class HandleServiceTemplates : HandleElementRequest<List<ServiceTemplate>>
{
    public HandleServiceTemplates(IToken token, string source)
        : base(token, source) { }
}

public class HandleServiceTemplate : HandleElementRequest<ServiceTemplate>
{
    public HandleServiceTemplate(IToken token, string source)
        : base(token, source) { }
}

public class HandleLayer : HandleElementRequest<Layer>
{
    public HandleLayer(IToken token, string source)
        : base(token, source) { }
}

public class HandleService : HandleElementRequest<Service>
{
    public HandleService(IToken token, string source)
        : base(token, source) { }
}

public partial class App
{
    private T? HandleElement<T>(HandleServiceTemplates request)
        where T : List<ServiceTemplate>
    {
        if (request.Token is not SpinnerToken tk || tk.Name != "Services")
        {
            return default(T);
        }

        List<ServiceTemplate> serviceTemplates = [];
        for (int i = 0; i < tk.Children.Length; i++)
        {
            if (tk.Children[i] is not SpinnerToken st)
            {
                continue;
            }

            var s = HandleElement<ServiceTemplate>(new(st, request.Source));

            if (s is null)
            {
                continue;
            }
            serviceTemplates.Add(s);
        }

        return (T)(object)serviceTemplates;
    }

    private T? HandleElement<T>(HandleServiceTemplate request)
        where T : ServiceTemplate
    {
        if (request.Token is not SpinnerToken token || token.Name != "Service")
        {
            return default(T);
        }

        var name = token.GetAttribute("name", request.Source) ?? "";
        if (string.IsNullOrEmpty(name))
        {
            return default(T);
        }

        var image = token.GetAttribute("image", request.Source) ?? "";
        var buildPath = token.GetAttribute("build", request.Source) ?? "";

        List<Layer> layers = [];

        List<Key> serviceTemplateKeys = HandleElement<List<Key>>(new(token, request.Source)) ?? [];

        for (int i = 0; i < token.Children.Length; i++)
        {
            if (token.Children[i] is not SpinnerToken stk)
            {
                continue;
            }

            var l = HandleElement<Layer>(new(stk, request.Source));
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
    }

    private T? HandleElement<T>(HandleLayer request)
        where T : Layer
    {
        if (request.Token is not SpinnerToken token || token.Name != "Layer")
        {
            return default(T);
        }

        var name = token.GetAttribute("name", request.Source) ?? "";
        if (string.IsNullOrEmpty(name))
        {
            return default(T);
        }

        var from = token.GetAttribute("from", request.Source) ?? "";
        List<IRun> layerCommands = [];

        for (int i = 0; i < token.Children.Length; i++)
        {
            if (token.Children[i] is not SpinnerToken tk)
            {
                continue;
            }

            var src = "";
            var dest = "";
            src = tk.GetAttribute("source", request.Source) ?? "";
            dest = tk.GetAttribute("dest", request.Source) ?? "";
            var filename = "";

            switch (tk.Name)
            {
                case "Copy":

                    if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(dest))
                    {
                        continue;
                    }

                    layerCommands.Add(new Copy(src, $"{dest}"));
                    break;
                case "Run":
                    var cmd = tk.GetAttribute("command", request.Source) ?? "";

                    if (!string.IsNullOrEmpty(cmd))
                    {
                        layerCommands.Add(new Run(cmd));
                        break;
                    }

                    if (tk.Children.Length == 0)
                    {
                        break;
                    }

                    layerCommands.Add(
                        new Run(
                            string.Join(
                                "",
                                tk.Children.Select(v =>
                                {
                                    if (v is XMLTextToken tx)
                                    {
                                        return string.Join(
                                            " ",
                                            tx.Lines.Select(x => x.Body.ToString(request.Source))
                                        );
                                    }
                                    return "";
                                })
                            )
                        )
                    );

                    break;
                case "Sql":
                    src = tk.GetAttribute("source", request.Source);
                    if (src is null)
                    {
                        return default(T);
                    }
                    filename = src.Split("/").Last().ToString();
                    layerCommands.Add(new Copy(src, $"/scripts"));
                    layerCommands.Add(
                        new Run(
                            "bash -c \"while ! pg_isready -U {{POSTGRES_USER}}; do sleep 2; done && psql -U {{POSTGRES_USER}} -d {{POSTGRES_DB}} "
                                + $" -f /scripts/{filename}\""
                        )
                    );

                    break;
            }
        }
        return (T)(object)new Layer(name, from: from, commands: layerCommands.ToArray());
    }

    private T? HandleElement<T>(HandleService request)
        where T : Service
    {
        if (request.Token is not SpinnerToken token || token.Name != "Service")
        {
            return default(T);
        }

        var name = token.GetAttribute("name", request.Source) ?? "";
        var layer = token.GetAttribute("layer", request.Source) ?? "";
        var target = token.GetAttribute("target", request.Source) ?? "false";
        bool.TryParse(target, out var tg);

        if (string.IsNullOrEmpty(name))
        {
            return default(T);
        }

        var serviceTemplate = ServiceManager.GetTemplate(name);
        if (serviceTemplate is null)
        {
            return default(T);
        }

        var serviceScope = serviceTemplate.Scope.Copy();

        serviceScope.Set("CONTAINER_NAME", "localhost", false, true);

        List<Arg> args = [];

        for (int i = 0; i < token.Children.Length; i++)
        {
            if (token.Children[i] is not SpinnerToken tk)
            {
                continue;
            }

            var arg = HandleElement<Arg>(new(tk, request.Source));
            if (arg is null)
            {
                continue;
            }

            args.Add(arg);
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

        var image = serviceTemplate.Image;

        if (string.IsNullOrEmpty(image))
        {
            image = $"sp-img-{serviceTemplate.Name}";
        }

        var service = new Service(
            serviceTemplate.Name,
            args: args.ToArray(),
            target: tg,
            image: image,
            scope: serviceScope,
            commands: serviceCommands.ToArray()
        );

        return (T)(object)service;
    }
}
