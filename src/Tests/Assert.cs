using System.Text;

namespace spinner;

public class AssertResult
{
    public bool Success { get; init; }
}

public interface ITestAssert
{
    AssertResult evaluate();
    string ToString(int depth);
}

public class TestAssert
{
    public ITestAssert[] Asserts { get; init; }

    public TestAssert(ITestAssert[] asserts)
    {
        Asserts = asserts;
    }

    public TestAssert()
    {
        Asserts = [];
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<Asserts>\n");

        builder.Append(string.Join("\n", Asserts.Select(v => v.ToString(depth + 1))));

        builder.Append($"\n{"".PadRight(4 * depth)}</Assert>");

        return builder.ToString();
    }
}

public class AssertEquals : ITestAssert
{
    public string Exptected { get; init; }
    public string Actual { get; init; }

    public AssertEquals(string exptected, string actual)
    {
        Exptected = exptected;
        Actual = actual;
    }

    public AssertEquals()
    {
        Exptected = "";
        Actual = "-";
    }

    public AssertResult evaluate()
    {
        return new() { Success = Actual == Exptected };
    }

    public string ToString(int depth)
    {
        return $"{"".PadRight(4 * depth)}<AssertEquals actual=\"{Actual}\" expected=\"{Exptected}\" />";
    }
}
