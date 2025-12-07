namespace spinner;

public class Arg
{
    public string Key { get; init; } = "";
    public string Value { get; set; } = "";
    public string FROM { get; set; } = "";

    public Arg() { }

    public Arg(string name, string value, string? from = null)
    {
        Key = name;
        Value = value;
        FROM = from ?? "";
    }

    public override string ToString()
    {
        return ToString(0);
    }

    public string ToString(int depth = 0)
    {
        var value = string.IsNullOrEmpty(Value) ? "" : $" value=\"{Value}\"";
        var from = string.IsNullOrEmpty(FROM) ? "" : $" from=\"{FROM}\"";
        return $"{"".PadRight(4 * depth)}<Arg key=\"{Key}\"{value}{from}/>";
    }
}
