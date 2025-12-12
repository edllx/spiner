using spinner;

namespace __Tests__;

public partial class TestInputs
{
    public static IEnumerable<Object[]> DiagonalComputationInputs =
    [
        new object[]
        {
            new int[] { 1, 2, 3, 4, 5 },
            new int[] { 1, 2, 3, 4, 5 },
            new Diag2[] { new(0, 0, 5) },
        },
        new object[]
        {
            new int[] { 1, 2, 7, 4, 5 },
            new int[] { 1, 2, 3, 4, 5 },
            new Diag2[] { new(0, 0, 2), new(3, 3, 2) },
        },
        new object[]
        {
            new int[] { 1, 2, 7, 1, 5 },
            new int[] { 1, 2, 3, 4, 5 },
            new Diag2[] { new(0, 0, 2), new(0, 3, 1), new(4, 4, 1) },
        },
        new object[]
        {
            new int[] { 1, 2, 7, 4, 5 },
            new int[] { 1, 2, 3, 1, 5 },
            new Diag2[] { new(0, 0, 2), new(3, 0, 1), new(4, 4, 1) },
        },
    ];
}
