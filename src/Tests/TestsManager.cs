using System.Text;

namespace spinner;

public partial class TestsManager
{
    public List<TestSuite> Tests { get; set; } = [];

    public TestsManager() { }

    public void SetTemplates(List<TestSuite>? tests)
    {
        if (tests is null)
        {
            return;
        }
        Tests = tests;
    }

    public string ToString(int depth = 0)
    {
        StringBuilder builder = new();

        builder.Append($"{"".PadRight(4 * depth)}<TestDescription>\n");
        builder.Append(string.Join("\n", Tests.Select(v => v.ToString(depth + 1))));
        builder.Append($"\n{"".PadRight(4 * depth)}</TestDescription>");

        return builder.ToString();
    }
}
