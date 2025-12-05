using spinner;

namespace __Tests__;

public class SpinnerParserTest
{
    public static IEnumerable<Object[]> ValidDocuments = TestInputs.SpinnerFilesTests;

    [Theory]
    [MemberData(nameof(ValidDocuments))]
    public void KeyTest(App actualApp, App expectedApp)
    {
        actualApp.Init();
        var expected = expectedApp.ToString();
        var actual = actualApp.ToString();

        Assert.Equal(expected, actual);
        Assert.True(
            expected == actual,
            $"{expectedApp.Args}\n\nExpected:\n{expected}\n\nActual:\n{actual}\n"
        );
    }
}
