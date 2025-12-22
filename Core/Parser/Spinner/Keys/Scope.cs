using System.Text;

namespace spinner;

public interface Iresovable
{
    void Resolve(Scope? scope = null);
}

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
            Keys.Add(
                new(s.Keys[i].Name, s.Keys[i].Value)
                {
                    Generated = s.Keys[i].Generated,
                    GenInfo = s.Keys[i].GenInfo.Copy(),
                }
            );
        }
    }

    public Scope Copy()
    {
        return new(this);
    }

    public Scope Combine(Scope scope)
    {
        var res = new Scope();
        foreach (var key in Keys)
        {
            if (string.IsNullOrEmpty(key.Value))
            {
                res.Set(key.Name, scope.Get(key.Name) ?? "", create: true);
                continue;
            }

            res.Set(key.Name, key.Value ?? "", create: true);
        }

        foreach (var key in scope.Keys)
        {
            if (res.Get(key.Name) is not null)
            {
                continue;
            }

            res.Set(key.Name, key.Value, create: true);
        }
        return res;
    }

    public Scope(IEnumerable<Key> keys, Scope? parent = null)
    {
        Parent = parent;
        foreach (Key item in keys)
        {
            Keys.Add(item.Copy());
        }
    }

    public void Set(params (string, string)[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            Set(keys[i].Item1, keys[i].Item2, bubble: false, create: true);
        }
    }

    public string? Get(string keyname)
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
            return null;
        }

        return Parent.Get(keyname);
    }

    public bool Set(string keyname, string value, bool bubble = true, bool create = false)
    {
        for (int i = 0; i < Keys.Count; i++)
        {
            if (Keys[i].Name == keyname)
            {
                Keys[i].Set(value);
                return true;
            }
        }

        if (create)
        {
            Keys.Add(new(keyname, value));
            return true;
        }

        if (!bubble)
        {
            return false;
        }

        if (Parent is null)
        {
            return false;
        }

        return Parent.Set(keyname, value);
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

    public void Resolve(Scope? scope = null)
    {
        try
        {
            KeyManager.Resolve(Keys.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
