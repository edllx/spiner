using System.Text;

namespace spinner;

public partial class TestSuite
{
    public Stack TestStack { get; init; }
    public Tests[] TestSet { get; init; } = [];

    public TestSuite(Stack testStack, Tests[] tests)
    {
        TestStack = testStack;
        TestSet = tests;
    }

    public TestSuite()
    {
        TestStack = new();
        TestSet = [];
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<TestSuite>\n");
        builder.Append(TestStack.ToString(depth + 1));
        if (TestSet.Length > 0)
        {
            builder.Append("\n");
            builder.Append(string.Join("\n", TestSet.Select(v => v.ToString(depth + 1))));
        }
        builder.Append($"\n{"".PadRight(4 * depth)}</TestSuite>");

        return builder.ToString();
    }
}
