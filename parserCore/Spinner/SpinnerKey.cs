using static spinner.Parser;

namespace spinner;

public class MissingKeyAttributeException(int position)
    : Exception($"Missing key name Pos: {position}") { }

internal class SpinnerKeyParser : IParser
{
    private static IParser Key = new XMLSingleLineElementParser("Key");
    private static IParser GeneratedKey = new XMLSingleLineElementParser("GeneratedKey");

    private static IParser Spaces = AnyStringP(" \t");
    private static IParser Service = Seq(Optional(Spaces), Choice(Key, GeneratedKey));

    public ParseResult Parse(ParseContext context)
    {
        int initialPosition = context.Position;
        var res = Service.Parse(context);

        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;
        XMLElementToken xmlElement = (XMLElementToken)seq.Children[1];

        Range name = new();
        Range value = new();
        Range len = new();

        for (int i = 0; i < xmlElement.Attributes.Length; i++)
        {
            var att = xmlElement.Attributes[i];
            switch (context.Input.AsSpan().Slice(att.Name.Start, att.Name.Length).ToString())
            {
                case "name":
                    name = att.Value;
                    break;

                case "value":
                    value = att.Value;
                    break;

                case "len":
                    len = att.Value;
                    break;

                default:
                    break;
            }
        }

        if (name.Length <= 0)
        {
            throw new MissingKeyAttributeException(initialPosition);
        }

        return ParseResult.SuccessAt(
            new SpinnerKeyToken()
            {
                KeyType = xmlElement.Name,
                Body = seq.Body,
                Name = name,
                Len = len,
                Value = value,
            }
        );
    }
}

public class SpinnerKeyToken : IToken
{
    public Range KeyType { get; init; }
    public Range Len { get; init; }
    public Range Body { get; init; }
    public Range Name { get; init; }
    public Range Value { get; init; }

    public string ToString(string source, int depth = 0)
    {
        var name = $"name=\"{source.AsSpan().Slice(Name.Start, Name.Length)}\"";
        var value = "";
        if (Value.Length > 0)
        {
            value = $"value=\"{source.AsSpan().Slice(Value.Start, Value.Length)}\"";
        }
        var keyType = $"{source.AsSpan().Slice(KeyType.Start, KeyType.Length)}";

        switch (keyType)
        {
            case "GeneratedKey":
                string len = $"len=\"{source.AsSpan().Slice(Len.Start, Len.Length)}\"";
                return $"{"".PadRight(4 * depth)}<{keyType} {name} {value} {len}/>";

            default:
                return $"{"".PadRight(4 * depth)}<{keyType} {name} {value}/>";
        }
    }
}
