using spinner;

namespace __Tests__;

[Collection("test-inputs")]
public class KeyResolutionTest
{
    TestInputs Inputs;

    public KeyResolutionTest(TestInputs inputs)
    {
        Inputs = inputs;
    }

    [Fact]
    public void ValueWithNoRef()
    {
        var testdata = Inputs.KeyResolutionInput[0];
        Key[] data = testdata.Keys.ToArray();
        KeyManager.Resolve(data);

        Assert.Equal<Key>(
            testdata.Expected,
            data,
            (a, b) => a.Name == b.Name && a.Value == b.Value
        );
    }

    [Fact]
    public void SingleLevelRef()
    {
        var testdata = Inputs.KeyResolutionInput[1];
        Key[] data = testdata.Keys.ToArray();
        KeyManager.Resolve(data);

        Assert.Equal<Key>(
            testdata.Expected,
            data,
            (a, b) => a.Name == b.Name && a.Value == b.Value
        );
    }

    [Fact]
    public void NestedRef()
    {
        var testdata = Inputs.KeyResolutionInput[2];
        Key[] data = testdata.Keys.ToArray();
        KeyManager.Resolve(data);

        Assert.Equal<Key>(
            testdata.Expected,
            data,
            (a, b) => a.Name == b.Name && a.Value == b.Value
        );
    }

    [Fact]
    public void MutipleRef()
    {
        var testdata = Inputs.KeyResolutionInput[3];
        Key[] data = testdata.Keys.ToArray();
        KeyManager.Resolve(data);

        Assert.Equal<Key>(
            testdata.Expected,
            data,
            (a, b) => a.Name == b.Name && a.Value == b.Value
        );
    }

    [Fact]
    public void MixedContentAndRef()
    {
        var testdata = Inputs.KeyResolutionInput[6];
        Key[] data = testdata.Keys.ToArray();
        KeyManager.Resolve(data);

        Assert.Equal<Key>(
            testdata.Expected,
            data,
            (a, b) => a.Name == b.Name && a.Value == b.Value
        );
    }

    [Fact]
    public void NestedRef2()
    {
        var testdata = Inputs.KeyResolutionInput[7];
        Key[] data = testdata.Keys.ToArray();
        KeyManager.Resolve(data);

        Assert.Equal<Key>(
            testdata.Expected,
            data,
            (a, b) => a.Name == b.Name && a.Value == b.Value
        );
    }

    [Fact]
    public void CircularRef()
    {
        var testdata = Inputs.KeyResolutionInput[4];
        Key[] data = testdata.Keys.ToArray();

        Assert.Throws<CircularReferenceException<string>>(() => KeyManager.Resolve(data));
    }

    [Fact]
    public void SelfRef()
    {
        var testdata = Inputs.KeyResolutionInput[5];
        Key[] data = testdata.Keys.ToArray();

        Assert.Throws<CircularReferenceException<string>>(() => KeyManager.Resolve(data));
    }

    [Fact]
    public void UndefinedRef()
    {
        var testdata = Inputs.KeyResolutionInput[8];
        Key[] data = testdata.Keys.ToArray();

        Assert.Throws<MissingKeyException>(() => KeyManager.Resolve(data));
    }

    [Fact]
    public void Complex1()
    {
        var testdata = Inputs.KeyResolutionInput[9];
        Key[] data = testdata.Keys.ToArray();
        KeyManager.Resolve(data);

        Assert.Equal<Key>(
            testdata.Expected,
            data,
            (a, b) => a.Name == b.Name && a.Value == b.Value
        );
    }

    [Fact]
    public void SpecialCharacter1()
    {
        var testdata = Inputs.KeyResolutionInput[10];
        Key[] data = testdata.Keys.ToArray();
        KeyManager.Resolve(data);

        Assert.Equal<Key>(
            testdata.Expected,
            data,
            (a, b) => a.Name == b.Name && a.Value == b.Value
        );
    }

    [Fact]
    public void EmptyKeyValues()
    {
        var testdata = Inputs.KeyResolutionInput[11];
        Key[] data = testdata.Keys.ToArray();
        KeyManager.Resolve(data);

        Assert.Equal<Key>(
            testdata.Expected,
            data,
            (a, b) => a.Name == b.Name && a.Value == b.Value
        );
    }
}
