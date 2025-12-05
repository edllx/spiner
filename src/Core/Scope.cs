using System.Text;

namespace spinner;

/* A Scope shoul provide utilities to
 * - Resolve Keys in the scope
 * - Get/Set keys value (bubbling up to parent scope
 *   if necessary)
 * */
public class Scope : Iresovable
{
    public Scope? Parent { get; set; }
    public List<Key> Keys { get; private set; } = [];

    public Scope() { }

    public Scope(Scope s)
    {
        if (s.Parent is not null)
        {
            Parent = new(s.Parent);
        }

        for (int i = 0; i < s.Keys.Count; i++)
        {
            Keys.Add(new(s.Keys[i].Name, s.Keys[i].Value));
        }
    }

    public Scope Copy()
    {
        return new(this);
    }

    public Scope(IEnumerable<Key> keys, Scope? parent = null)
    {
        Parent = parent;
        foreach (Key item in keys)
        {
            Keys.Add(new(item.Name, item.Value));
        }
    }

    public void Fill(SpinnerToken token, string source)
    {
        Keys.Clear();
        for (int i = 0; i < token.Children.Length; i++)
        {
            var child = token.Children[i];

            try
            {
                var key = Key.Build(child, source);
                Set((key.Name, key.Value));
            }
            catch (Exception) { }
        }
    }

    public void Set(params (string, string)[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            Set(keys[i].Item1, keys[i].Item2, bubble: false, create: true);
        }
    }

    public string Get(string keyname)
    {
        for (int i = 0; i < Keys.Count; i++)
        {
            if (Keys[i].Name == keyname)
            {
                return Keys[i].Value;
            }
        }
        if (Parent is null)
        {
            throw new MissingKeyException(keyname);
        }

        return Parent.Get(keyname);
    }

    public void Set(string keyname, string value, bool bubble = true, bool create = false)
    {
        for (int i = 0; i < Keys.Count; i++)
        {
            if (Keys[i].Name == keyname)
            {
                Keys[i].Set(value);
                return;
            }
        }

        if (!bubble)
        {
            if (create)
            {
                Keys.Add(new(keyname, value));
            }
            return;
        }

        if (Parent is null)
        {
            throw new MissingKeyException(keyname);
        }

        Parent.Set(keyname, value);
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();
        builder.Append($"{"".PadRight(4 * depth)}<Keys>\n");
        builder.Append(
            string.Join(
                "\n",
                Keys.Select(v =>
                {
                    return $"{"".PadRight(4 * (depth + 1))}{v.Name}: \"{v.Value}\"";
                })
            )
        );

        builder.Append($"\n{"".PadRight(4 * depth)}</Keys>");

        return builder.ToString();
    }

    public void Resolve(Scope? scope)
    {
        KeyManager.Resolve(Keys.ToArray());
    }
}
