using System.Text;

namespace spinner;

public class Layer : Iresovable
{
    public string Name { get; init; }
    public string From { get; init; }
    private IRun[] _commands = [];
    public IRun[] Commands
    {
        get { return _commands; }
        init { _commands = value; }
    }

    private (Type, int)[] CommandPriority = [(typeof(Copy), 1), (typeof(Run), 2)];

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

    public void Sort()
    {
        var cmds = Commands.ToList();

        cmds.Sort(
            (a, b) =>
            {
                Type typeA = a.GetType();
                Type typeB = b.GetType();

                (Type, int)? p1 = null;
                (Type, int)? p2 = null;

                for (int i = 0; i < CommandPriority.Length; i++)
                {
                    if (typeA == CommandPriority[i].Item1)
                    {
                        p1 = CommandPriority[i];
                    }
                    if (typeB == CommandPriority[i].Item1)
                    {
                        p2 = CommandPriority[i];
                    }
                }

                int priorityA = p1?.Item2 ?? int.MaxValue;
                int priorityB = p2?.Item2 ?? int.MaxValue;

                return priorityA.CompareTo(priorityB);
            }
        );
        _commands = cmds.ToArray();
    }

    public override string ToString()
    {
        return ToString(0);
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

    public static Layer[] ResolveLayer(Layer[] layers)
    {
        Layer[] lys = new Layer[layers.Length];
        List<int>[] adj = new List<int>[layers.Length];
        List<(int, int)> dependencies = [];

        for (int i = 0; i < adj.Length; i++)
        {
            adj[i] = [];
        }

        for (int i = 0; i < layers.Length; i++)
        {
            var el = layers[i];
            var from = el.From.Split(",");

            for (int j = 0; j < from.Length; j++)
            {
                if (string.IsNullOrEmpty(from[j]))
                {
                    continue;
                }
                int idx = -1;
                for (int k = 0; k < layers.Length; k++)
                {
                    if (layers[k].Name != from[j])
                    {
                        continue;
                    }
                    idx = k;
                }
                if (idx < 0 || idx == i)
                {
                    continue;
                }

                dependencies.Add((i, idx));

                adj[i].Add(idx);
            }
        }

        int[] topo = Tools.TopoSort(dependencies.ToArray(), layers.Length);

        for (int i = 0; i < topo.Length; i++)
        {
            int layerIdx = topo[i];
            Layer template = layers[layerIdx];
            List<IRun> runs = [];
            bool[] included = new bool[layers.Length];

            ResolveLayer(layerIdx, runs, included, layers, adj);

            lys[layerIdx] = new Layer(template.Name, from: template.From, commands: runs.ToArray());
            lys[layerIdx].Sort();
        }

        return lys.ToArray();
    }

    private static void ResolveLayer(
        int idx,
        List<IRun> runs,
        bool[] included,
        Layer[] layers,
        List<int>[] adj
    )
    {
        for (int i = 0; i < adj[idx].Count; i++)
        {
            int j = adj[idx][i];
            if (included[j] || idx == j)
            {
                continue;
            }
            ResolveLayer(j, runs, included, layers, adj);
        }

        for (int i = 0; i < layers[idx].Commands.Length; i++)
        {
            runs.Add(layers[idx].Commands[i].Copy());
        }

        included[idx] = true;
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
