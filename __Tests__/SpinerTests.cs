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
        var expected = expectedApp.ToString(0);
        var actual = actualApp.ToString(0);

        Assert.Equal(expected, actual);
    }
}
