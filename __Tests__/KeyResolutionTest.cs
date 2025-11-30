using spinner;

namespace __Tests__;

public class KeyResolutionTest
{
    public static IEnumerable<Object[]> KeyResolutionIntputs = TestInputs.KeyResolutionInput;
    public static IEnumerable<Object[]> KeyResolutionIntputsThows =
        TestInputs.KeyResolutionInputShoulThrow;

    [Theory]
    [MemberData(nameof(KeyResolutionIntputs))]
    public void KeyTest(Key[] data, Key[] resolved)
    {
        KeyManager.Resolve(data);
        Assert.Equal<Key>(resolved, data, (a, b) => a.Name == b.Name && a.Value == b.Value);
    }

    [Theory]
    [MemberData(nameof(KeyResolutionIntputsThows))]
    public void KeyTestThrows(Key[] data, Exception ex)
    {
        Assert.Throws(ex.GetType(), () => KeyManager.Resolve(data));
    }
}
