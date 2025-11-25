using System.Text;
using static spinner.Parser;

namespace spinner;

public class SpinnerParser
{
    private static SpinnerElement SpinnerKey = new("Key")
    {
        Mode = ElementMode.SingleLine,
        IsAttributeAllowed = true,
    };

    private static SpinnerElement SpinnerGeneratedKey = new("GeneratedKey")
    {
        Mode = ElementMode.SingleLine,
        IsAttributeAllowed = true,
    };

    private static SpinnerElement Set = new("Set")
    {
        Mode = ElementMode.SingleLine,
        IsAttributeAllowed = true,
    };

    private static SpinnerElement LayerSQL = new("Sql")
    {
        Mode = ElementMode.SingleLine,
        IsAttributeAllowed = true,
    };

    private static SpinnerElement LayerCopy = new("Copy")
    {
        Mode = ElementMode.SingleLine,
        IsAttributeAllowed = true,
    };

    private static SpinnerElement LayerRun = new("Run")
    {
        Mode = ElementMode.Both,
        IsAttributeAllowed = true,
        AllowedChildElements = [XML.Text],
    };

    private static SpinnerElement Layer = new("Layer")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = true,
        AllowedChildElements = [LayerSQL, LayerCopy, LayerRun],
    };

    private static SpinnerElement Service = new("Service")
    {
        Mode = ElementMode.Both,
        IsAttributeAllowed = true,
        AllowedChildElements = [SpinnerKey, SpinnerGeneratedKey, Layer],
    };

    private static SpinnerElement Services = new("Services")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = false,
        AllowedChildElements = [Service],
    };

    public static SpinnerElement RequestBody = new("Body")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = true,
        AllowedChildElements = [SpinnerKey],
    };

    public static SpinnerElement Request = new("Request")
    {
        Mode = ElementMode.Both,
        IsAttributeAllowed = true,
        AllowedChildElements = [SpinnerKey, RequestBody],
    };

    public static SpinnerElement Requests = new("Requests")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = false,
        AllowedChildElements = [Request],
    };

    private static SpinnerElement Arg = new("Arg")
    {
        Mode = ElementMode.SingleLine,
        IsAttributeAllowed = true,
    };

    private static SpinnerElement StackService = new("Service")
    {
        Mode = ElementMode.Both,
        IsAttributeAllowed = true,
        AllowedChildElements = [Arg],
    };

    public static SpinnerElement TestStack = new("Stack")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = false,
        AllowedChildElements = [StackService],
    };

    public static SpinnerElement TestRequest = new("Request")
    {
        Mode = ElementMode.Both,
        IsAttributeAllowed = true,
        AllowedChildElements = [Arg],
    };

    private static SpinnerElement AssertNotNull = new("NotNull")
    {
        Mode = ElementMode.SingleLine,
        IsAttributeAllowed = false,
    };

    private static SpinnerElement AssertEquals = new("Equals")
    {
        Mode = ElementMode.SingleLine,
        IsAttributeAllowed = true,
    };

    private static SpinnerElement Asserts = new("Asserts")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = false,
        AllowedChildElements = [AssertNotNull, AssertEquals],
    };

    private static SpinnerElement TestResponse = new("Response")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = false,
        AllowedChildElements = [Set],
    };

    private static SpinnerElement Test = new("Test")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = false,
        AllowedChildElements = [SpinnerKey, TestRequest, TestResponse, Asserts],
    };

    private static SpinnerElement Tests = new("Tests")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = true,
        AllowedChildElements = [Test],
    };

    public static SpinnerElement TestSuite = new("TestSuite")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = false,
        AllowedChildElements = [TestStack, Tests],
    };

    private static SpinnerElement Spinner = new("Spinner")
    {
        Mode = ElementMode.MultiLine,
        IsAttributeAllowed = false,
        AllowedChildElements = [Services, Requests, TestSuite],
    };

    public ParseResult Parse(string source)
    {
        try
        {
            var context = new ParseContext(source);
            var res = Spinner.Parse(context);
            if (!res.Success)
            {
                return res;
            }

            SpinnerToken spinner = (SpinnerToken)res.Token;

            return ParseResult.SuccessAt(spinner);
        }
        catch (MissingKeyAttributeException ex)
        {
            Console.WriteLine(ex.Message);
            return ParseResult.FailAt(new ParseFailedToken());
        }
    }
}

public enum ElementMode
{
    MultiLine,
    SingleLine,
    Both,
}

public class SpinnerElement : IParser
{
    public string Name { get; init; }
    public ElementMode Mode { get; init; } = ElementMode.MultiLine;
    public bool IsAttributeAllowed { get; init; }
    public IParser[] AllowedChildElements { get; init; } = [];

    private static IParser Spaces = AnyStringP(" \t");
    private static IParser Comments = new XMLCommentParser();

    private IParser Element = Any;
    private bool Initialized = false;

    public SpinnerElement(string name)
    {
        Name = name;
    }

    private void Init()
    {
        if (Initialized)
        {
            return;
        }

        IParser closingTag = XML.ClosingTag(Name);
        IParser text = ConsumeUntil(closingTag);
        List<IParser> allowedChild = [];

        allowedChild.Add(LineBreak);
        allowedChild.Add(Comments);
        for (int i = 0; i < AllowedChildElements.Length; i++)
        {
            allowedChild.Add(AllowedChildElements[i]);
        }
        allowedChild.Add(XML.GenericElement);
        allowedChild.Add(text);

        IParser Body = ZeroPlus(Choice(allowedChild.ToArray()));

        IParser SingleLineElement = new XMLSingleLineElementParser(Name);
        IParser MultilineElement = new XMLElemenParser(Name, Body);

        switch (Mode)
        {
            case ElementMode.SingleLine:
                Element = Seq(Optional(Spaces), SingleLineElement);
                break;

            case ElementMode.MultiLine:
                Element = Seq(Optional(Spaces), MultilineElement);
                break;

            case ElementMode.Both:
                Element = Seq(Optional(Spaces), Choice(SingleLineElement, MultilineElement));
                break;
            default:
                break;
        }
    }

    public ParseResult Parse(ParseContext context)
    {
        Init();
        int initialPosition = context.Position;
        var res = Element.Parse(context);
        if (!res.Success)
        {
            return res;
        }

        SequenceToken seq = (SequenceToken)res.Token;
        XMLElementToken xmlElement = (XMLElementToken)seq.Children[1];

        return ParseResult.SuccessAt(
            new SpinnerToken()
            {
                Name = Name,
                Body = xmlElement.Body,
                Children = xmlElement.Children,
                Attributes = IsAttributeAllowed ? xmlElement.Attributes : [],
                Comments = xmlElement.Comments,
                Mode = Mode,
            }
        );
    }
}

public class SpinnerToken : IToken
{
    public required string Name { get; init; }
    public ElementMode Mode { private get; init; } = ElementMode.MultiLine;
    public Range Body { get; init; }
    public XMLAttributeToken[] Attributes { get; init; } = [];
    public XMLCommentToken[] Comments { get; init; } = [];
    public IToken[] Children { get; init; } = [];

    public string ToString(string source, int depth = 0)
    {
        switch (Mode)
        {
            case ElementMode.SingleLine:
                return ToStringSindleLine(source, depth);

            case ElementMode.MultiLine:
                return ToStringMultiLine(source, depth);

            case ElementMode.Both:
                if (Children.Length > 0)
                {
                    return ToStringMultiLine(source, depth);
                }

                return ToStringSindleLine(source, depth);
        }

        return ToStringMultiLine(source, depth);
    }

    private string ToStringSindleLine(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        for (int i = 0; i < Attributes.Length; i++)
        {
            var el = Attributes[i];
            buffer.Append($"{el.Name.ToString(source)}=\"{el.Value.ToString(source)}\"");
            if (i < Attributes.Length - 1)
            {
                buffer.Append(" ");
            }
        }

        return $"{"".PadRight(4 * depth)}<{Name} {buffer}/>";
    }

    private string ToStringMultiLine(string source, int depth = 0)
    {
        var buffer = new StringBuilder();

        buffer.Append($"{"".PadRight(4 * depth)}<{Name}>\n");
        buffer.Append(string.Join('\n', Attributes.Select(el => el.ToString(source, depth + 1))));
        if (Attributes.Length > 0 && Children.Length > 0)
        {
            buffer.Append("\n");
        }
        buffer.Append(string.Join('\n', Children.Select(el => el.ToString(source, depth + 1))));
        buffer.Append($"\n{"".PadRight(4 * depth)}</{Name}>");

        return buffer.ToString();
    }
}
