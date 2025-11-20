using System.Text;

namespace __Tests__;

public interface ITestToken
{
    string ToString(string source, int depth = 0);
}

public class SpinnerTestToken : ITestToken
{
    public ITestToken[] Children { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Spinner>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Spinner>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}

public class XMLCommentTestToken : ITestToken
{
    public ITestToken[] Children { get; init; } = [];
    public Range Body { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Comment>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Comment>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}

public class TextTestToken() : ITestToken
{
    public string Body { get; init; } = "";

    public string ToString(string source, int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<Text Content='{Body}'/>";
    }
}

public class ServicesTestToken : ITestToken
{
    public ITestToken[] Children { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Services>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Services>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}

public class ServiceTestToken : ITestToken
{
    public ITestToken[] Children { get; init; } = [];
    public XMLAttributeTestToken[] Attributes { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Service>\n";
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Service>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}

public class XMLAttributeTestToken : ITestToken
{
    public string Name { get; init; } = "";
    public string Value { get; init; } = "";

    public string ToString(string source, int depth = 0)
    {
        return $"{"".PadRight(4 * depth)}<Attribute name=\"{Name}\" value=\"{Value}\"/>";
    }
}

public class XMLAttributesTestToken : ITestToken
{
    public Range Body { get; init; }
    public XMLAttributeTestToken[] Tokens { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        var lfMark = $"{"".PadRight(4 * depth)}<Attributes>\n";
        buffer.Append(string.Join('\n', Tokens.Select(el => el.ToString(source, depth + 1))));
        var rgMark = $"\n{"".PadRight(4 * depth)}</Attributes>";

        var body = $"{lfMark}{buffer}{rgMark}";
        return body;
    }
}

public class SpinnerKeyTestToken : ITestToken
{
    public string KeyType { get; init; } = "";
    public string Len { get; init; } = "";
    public string Body { get; init; } = "";
    public string Name { get; init; } = "";
    public string Value { get; init; } = "";

    public string ToString(string source, int depth = 0)
    {
        var name = $"name=\"{Name}\"";
        var value = "";
        if (Value.Length > 0)
        {
            value = $"value=\"{Value}\"";
        }
        var keyType = $"{KeyType}";

        switch (keyType)
        {
            case "GeneratedKey":
                string len = $"len=\"{Len}\"";
                return $"{"".PadRight(4 * depth)}<{keyType} {name} {value} {len}/>";

            default:
                return $"{"".PadRight(4 * depth)}<{keyType} {name} {value}/>";
        }
    }
}
