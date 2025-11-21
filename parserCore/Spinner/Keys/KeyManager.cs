using System.Text;

namespace spinner;

public class Key
{
    public string Name { get; init; }
    public string Value { get; private set; }
    public bool Resolved { get; private set; }

    public Key(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public void Resolve(string value)
    {
        if (Resolved)
        {
            return;
        }

        Value = value;

        Resolved = true;
    }

    public override string ToString()
    {
        return $"{Name} : {Value}";
    }
}

public class MissingKeyException(string keyName) : Exception($"Missing Key: {keyName}") { }

public class DuplicateKeyException(string keyName) : Exception($"Duplicate Key: {keyName}") { }

public class CircularReferenceException<T>(T[] keys) : Exception(string.Join(",", keys))
{
    public T[] Keys { get; init; } = keys;
}

public class KeyManager
{
    /// <summary>
    /// Resolve all the key present in keys
    /// </summary>
    /// <exception cref="MissingKeyException"/>
    /// <exception cref="DuplicateKeyException"/>
    /// <exception cref="CircularReferenceException"/>
    /// <param name="keys"></param>
    public static void Resolve(Key[] keys)
    {
        KeyParser parser = new();
        Dictionary<string, (int, KeyToken)> dictionary = [];

        for (int i = 0; i < keys.Length; i++)
        {
            var res = parser.Parse(keys[i].Value);
            var tks = res.Token as KeyToken;
            if (tks is null)
            {
                continue;
            }
            dictionary.Add(keys[i].Name, (i, tks));
        }

        List<(int, int)> nb = GenerateNb(dictionary, keys);
        try
        {
            var order = Tools.TopoSort(nb.ToArray(), dictionary.Count);
            Resolve(dictionary, keys, order);
        }
        catch (CircularReferenceException<int> ex)
        {
            throw new CircularReferenceException<string>([.. ex.Keys.Select(v => keys[v].Name)]);
        }

        return;
    }

    private static void Resolve(
        Dictionary<string, (int, KeyToken)> dictionary,
        Key[] keys,
        int[] order
    )
    {
        for (int i = 0; i < order.Length; i++)
        {
            Key k = keys[i];

            if (!dictionary.TryGetValue(k.Name, out var l))
            {
                throw new MissingKeyException(k.Name);
            }

            string resovedValue = string.Join(
                "",
                l.Item2.Tokens.Select(val =>
                {
                    var buffer = new StringBuilder();
                    switch (val)
                    {
                        case TextToken text:
                            buffer.Append(
                                k.Value.AsSpan().Slice(text.Body.Start, text.Body.Length)
                            );
                            break;

                        case KeyRefToken t:

                            var name = k
                                .Value.AsSpan()
                                .Slice(t.Name.Start, t.Name.Length)
                                .ToString();

                            if (!dictionary.TryGetValue(name, out var ll))
                            {
                                throw new MissingKeyException(name);
                            }

                            Key v = keys[ll.Item1];
                            buffer.Append(v.Value);
                            break;

                        default:
                            break;
                    }
                    return buffer.ToString();
                })
            );
            k.Resolve(resovedValue);
        }
    }

    private static List<(int, int)> GenerateNb(
        Dictionary<string, (int, KeyToken)> dictionary,
        Key[] keys
    )
    {
        List<(int, int)> res = [];
        foreach (var x in dictionary)
        {
            foreach (var k in x.Value.Item2.Tokens)
            {
                if (k is not KeyRefToken kt)
                {
                    continue;
                }
                var keyIdx = x.Value.Item1;

                var key = keys[keyIdx]
                    .Value.AsSpan()
                    .Slice(kt.Name.Start, kt.Name.Length)
                    .ToString();

                if (!dictionary.ContainsKey(key))
                {
                    throw new MissingKeyException(key);
                }

                var keyMap = dictionary[key];

                res.Add((keyIdx, keyMap.Item1));
            }
        }
        return res;
    }
}
