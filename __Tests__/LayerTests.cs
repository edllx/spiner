using spinner;

namespace __Tests__;

public class LayerResolutionTests
{
    public static IEnumerable<Object[]> LayerTests = TestInputs.LayerTests;

    [Theory]
    [MemberData(nameof(LayerTests))]
    public void KeyTest(Layer[] inputLayers, Layer[] expectedLayers)
    {
        var input = string.Join("\n", inputLayers.Select(v => v.ToString(0)));
        var expected = string.Join("\n", expectedLayers.Select(v => v.ToString(0)));
        var resolvedLayer = Layer.ResolveLayer(inputLayers);
        var actual = string.Join("\n", resolvedLayer.Select(v => v.ToString(0)));

        //Assert.Equal(expected, actual);
        Assert.True(expected == actual, $"{input}\n\nExpected:\n{expected}\n\nActual:\n{actual}\n");
    }
}
