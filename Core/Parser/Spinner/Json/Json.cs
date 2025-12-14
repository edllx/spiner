using System.Text.Json;
using static spinner.Parser;

namespace spinner;

public struct JsonValue
{
    public bool Found { get; init; }
    public required string Path { get; init; }
    public required string Value { get; init; }
}

public struct JsonResponse
{
    public bool Found { get; init; }
    public required string Path { get; init; }
    public string Key { get; init; }
    public string Value { get; init; }
    public JsonResponseOperatorTokenType Type { get; init; }

    public override string ToString()
    {
        return $"Path:{Path}\nFound:{Found}\nKey:{Key}\nValue:{Value}\nType:{Type}";
    }
}

public class JsonParser
{
    private static IParser Spaces = AnyStringP(" \t");
    public static IParser Operator = new JsonOperatorParser();
    public static IParser ResponseOperator = new JsonResponseOperatorParser();

    public static JsonValue Find(string path, JsonDocument document)
    {
        if (string.IsNullOrEmpty(path))
        {
            return new() { Path = path ?? "", Value = "" };
        }

        var ops = ExtractOperators(path);
        string? value = Find(ops, 0, document.RootElement, path);

        return new()
        {
            Path = path,
            Value = value ?? "",
            Found = value is not null,
        };
    }

    public static JsonOperatorToken[] ExtractOperators(string path)
    {
        IParser Element = ZeroPlus(Seq(Optional(Spaces), Operator));

        var res = Element.Parse(new ParseContext(path));

        if (!res.Success)
        {
            return [];
        }

        List<JsonOperatorToken> tokens = [];
        var seq = (SequenceToken)res.Token;
        Unroll(seq, tokens);

        return tokens.ToArray();
    }

    private static void Unroll(IToken token, List<JsonOperatorToken> tokens)
    {
        switch (token)
        {
            case JsonOperatorToken tk:
                tokens.Add(tk);
                break;

            case SequenceToken tk:
                for (int i = 0; i < tk.Children.Length; i++)
                {
                    Unroll(tk.Children[i], tokens);
                }
                break;

            default:
                break;
        }
    }

    private static string? Find(
        JsonOperatorToken[] ops,
        int idx,
        JsonElement element,
        string source
    )
    {
        if (idx < 0 || idx >= ops.Length)
        {
            return null;
        }

        var value = ops[idx].Value.ToString(source);

        try
        {
            switch (ops[idx].Type)
            {
                case JsonOperatorType.MemberAccess:
                    if (idx == ops.Length - 1)
                    {
                        return element.GetProperty(value).ToString();
                    }
                    return Find(ops, idx + 1, element.GetProperty(value), source);

                case JsonOperatorType.ArrayIndex:
                    var array = element.EnumerateArray();

                    if (value.Length > 1)
                    {
                        value = value.TrimStart('0');
                    }

                    var index = int.Parse(value);

                    var elem = array.ElementAt(index);

                    if (idx == ops.Length - 1)
                    {
                        return elem.ToString();
                    }
                    return Find(ops, idx + 1, elem, source);

                case JsonOperatorType.MetadataAccess:

                    switch (value.ToLower())
                    {
                        case "length":
                            switch (element.ValueKind)
                            {
                                case JsonValueKind.Array:
                                    var arr = element.EnumerateArray();
                                    return $"{arr.Count()}";

                                case JsonValueKind.String:
                                    return $"{element.ToString().Count()}";

                                default:
                                    break;
                            }
                            break;

                        case "type":

                            switch (element.ValueKind)
                            {
                                case JsonValueKind.Array:
                                    return "array";

                                case JsonValueKind.String:
                                    return "string";

                                case JsonValueKind.True:
                                case JsonValueKind.False:
                                    return "boolean";

                                case JsonValueKind.Object:
                                    return "object";

                                case JsonValueKind.Number:
                                    return "number";

                                default:
                                    break;
                            }
                            break;
                    }

                    break;
            }
        }
        catch (Exception)
        {
            //Console.WriteLine(ex);
        }

        return null;
    }
}
