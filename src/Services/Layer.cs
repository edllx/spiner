using System.Text;

namespace spinner;

public class Layer : Iresovable
{
    public string Name { get; init; }
    public string From { get; init; }
    public IRun[] Commands { get; init; }

    public Layer(string name, string? from = null, IRun[]? commands = null)
    {
        Name = name;
        From = from ?? "";
        Commands = commands ?? [];
    }

    public Layer()
    {
        Name = "";
        From = "";
        Commands = [];
    }

    public override string ToString()
    {
        return ToString(0);
    }

    public static Layer Build(IToken token, string source)
    {
        if (token is not SpinnerToken tk || tk.Name != "Layer")
        {
            throw new Exception("THis is not a valid Layer token");
        }

        var name = tk
            .Attributes.FirstOrDefault(v => v.Name.ToString(source) == "name")
            ?.Value.ToString(source);
        var from = tk
            .Attributes.FirstOrDefault(v => v.Name.ToString(source) == "from")
            ?.Value.ToString(source);

        if (string.IsNullOrEmpty(name))
        {
            throw new Exception("Empty layer name");
        }

        List<IRun> commands = [];

        for (int i = 0; i < tk.Children.Length; i++)
        {
            var el = tk.Children[i];
            if (el is not SpinnerToken eltk)
            {
                continue;
            }
            var src = "";
            var dest = "";
            switch (eltk.Name)
            {
                case "Copy":
                    src = eltk
                        .Attributes.FirstOrDefault(v => v.Name.ToString(source) == "source")
                        ?.Value.ToString(source);

                    dest = eltk
                        .Attributes.FirstOrDefault(v => v.Name.ToString(source) == "source")
                        ?.Value.ToString(source);

                    if (string.IsNullOrEmpty(src))
                    {
                        throw new Exception("Empty Copy source");
                    }

                    if (string.IsNullOrEmpty(dest))
                    {
                        throw new Exception("Empty Copy destination");
                    }

                    commands.Add(new Copy(src, dest));
                    break;

                case "Run":
                    var cmd = eltk
                        .Attributes.FirstOrDefault(v => v.Name.ToString(source) == "command")
                        ?.Value.ToString(source);

                    if (!string.IsNullOrEmpty(cmd))
                    {
                        commands.Add(new Run(cmd));
                        break;
                    }

                    if (eltk.Children.Length == 0)
                    {
                        break;
                    }

                    commands.Add(
                        new Run(
                            string.Join(
                                "",
                                eltk.Children.Select(v =>
                                {
                                    if (v is XMLTextToken tx)
                                    {
                                        return string.Join(
                                            " ",
                                            tx.Lines.Select(x => x.Body.ToString(source))
                                        );
                                    }
                                    return "";
                                })
                            )
                        )
                    );
                    break;
                case "Sql":

                    src = eltk
                        .Attributes.FirstOrDefault(v => v.Name.ToString(source) == "source")
                        ?.Value.ToString(source);

                    if (string.IsNullOrEmpty(src))
                    {
                        throw new Exception("Empty Copy source");
                    }

                    commands.Add(new Copy(src, "/srcipts"));
                    var filename = src.Split("/").Last().ToString();
                    // TODO Support other sql dialect
                    commands.Add(new Run("psql -U {{POSTGRES_USER}} " + $"-f /script/{filename}"));
                    break;
            }
        }

        return new Layer(name, from: from, commands: commands.ToArray());
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();
        var from = string.IsNullOrEmpty(From) ? "" : $" from=\"{From}\"";

        builder.Append($"{"".PadRight(4 * depth)}<Layer name=\"{Name}\"{from}>\n");

        builder.Append(string.Join("\n", Commands.Select(v => v.ToString(depth + 1))));

        builder.Append($"\n{"".PadRight(4 * depth)}</Layer>");

        return builder.ToString();
    }

    public void Resolve(Scope? scope = null)
    {
        for (int i = 0; i < Commands.Length; i++)
        {
            Commands[i].Resolve(scope);
        }
    }
}

public interface IRun : Iresovable
{
    string ToString(int depth);
    IRun Copy();
}

public class Copy : IRun
{
    public string Source;
    public string Destination;

    public Copy(string source, string destination)
    {
        Source = source;
        Destination = destination;
    }

    public void Resolve(Scope? scope = null) { }

    public string ToString(int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<Copy source=\"{Source}\" dest=\"{Destination}\"/>";
    }

    IRun IRun.Copy()
    {
        return new Copy(Source, Destination);
    }
}

public class Run : IRun
{
    public string Text { get; set; }

    public Run(string text)
    {
        Text = text;
    }

    public void Resolve(Key[] keys) { }

    public string ToString(int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<Run Command=\"{Text}\"/>";
    }

    public void Resolve(Scope? scope = null)
    {
        if (scope is null)
        {
            return;
        }

        Text = KeyManager.Resolve(Text, scope.Keys.ToArray());
    }

    public IRun Copy()
    {
        return new Run(Text);
    }
}
