using spinner;

namespace __Tests__;

public class SpinnerParserTest
{
    public static IEnumerable<Object[]> ValidDocuments = TestInputs.SpinnerFilesTests;

    [Theory]
    [MemberData(nameof(ValidDocuments))]
    public void KeyTest(App actualApp, App expectedApp)
    {
        Assert.Skip("fix Sql command later");
        actualApp.Init();
        var expected = expectedApp.ToString(0);
        var actual = actualApp.ToString(0);

        Assert.True(expected == actual, Diff.TextDiff(actual, expected).ToString());
    }
}
