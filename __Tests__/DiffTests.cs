using spinner;

namespace __Tests__;

public class DiffTests
{
    public static IEnumerable<Object[]> DiagComputationInputs =
        TestInputs.DiagonalComputationInputs;

    [Theory]
    [MemberData(nameof(DiagComputationInputs))]
    public void ClITes(int[] expectedHash, int[] actualHash, Diag2[] expected)
    {
        var actual = Diff.GetDiags(expectedHash, actualHash);

        Assert.Equal(
            expected,
            actual,
            (a, b) =>
            {
                if (a.Length != b.Length)
                {
                    return false;
                }
                for (int i = 0; i < a.Length; i++)
                {
                    if (!a[i].Equals(b[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        );
    }
}
