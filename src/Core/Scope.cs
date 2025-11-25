namespace spinner;

/* A Scope shoul provide utilities to
 * - Resolve Keys in the scope
 * - Get/Set keys value (bubbling up to parent scope
 *   if necessary)
 * */
public class Scope
{
    public Scope? Parent { private get; init; }
    private List<Key> Keys = [];

    public void Set(params (string, string)[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            Set(keys[i].Item1, keys[i].Item1, bubble: false, create: true);
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
}
