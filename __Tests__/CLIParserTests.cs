using spinner;

namespace __Tests__;

public class CLIParserTests
{
    public static IEnumerable<object[]> TestData => TestInputs.CLIIntputs;
    public static IEnumerable<object[]> TestAppThrows => TestInputs.AppInitShouldThrow;
    private static CLIArgParser Parser = new();

    [Theory]
    [MemberData(nameof(TestData))]
    public void ClITes(IParser command, string args, CommandToken token)
    {
        var res = command.Parse(new ParseContext(args));

        Assert.True(res.Success, "Parsing failed");
        var expected = token.ToString(args);
        var actual = res.ToString(args);
        Assert.True(expected == actual, $"{args}\n\nExpected:\n{expected}\n\nActual:\n{actual}\n");
    }

    [Theory]
    [MemberData(nameof(TestAppThrows))]
    public void AppCreation(string args, Exception ex)
    {
        Assert.Throws(
            ex.GetType(),
            () =>
            {
                var app = new App(args);
                app.Init();
            }
        );
    }
}
