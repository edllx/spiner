using spinner;

namespace __Tests__;

public class ExpectedResolution
{
    public Key[] Keys { get; init; } = [];
    public Key[] Expected { get; set; } = [];

    public static ExpectedResolution[] TestData()
    {
        return
        [
            // Case 1: Simple values without references
            new()
            {
                Keys = [new Key("name", "John"), new Key("age", "30"), new Key("city", "New York")],
                Expected =
                [
                    new Key("name", "John"),
                    new Key("age", "30"),
                    new Key("city", "New York"),
                ],
            },
            // Case 2: Single level references
            new()
            {
                Keys =
                [
                    new Key("first_name", "John"),
                    new Key("last_name", "Doe"),
                    new Key("full_name", "{{first_name}} {{last_name}}"),
                ],
                Expected =
                [
                    new Key("first_name", "John"),
                    new Key("last_name", "Doe"),
                    new Key("full_name", "John Doe"),
                ],
            },
            // Case 3: Nested references
            new()
            {
                Keys =
                [
                    new Key("a", "Hello"),
                    new Key("b", "{{a}} World"),
                    new Key("c", "{{b}}!!!"),
                ],
                Expected =
                [
                    new Key("a", "Hello"),
                    new Key("b", "Hello World"),
                    new Key("c", "Hello World!!!"),
                ],
            },
            // Case 4: Multiple references in one value
            new()
            {
                Keys =
                [
                    new Key("protocol", "https"),
                    new Key("domain", "example.com"),
                    new Key("path", "api/v1"),
                    new Key("url", "{{protocol}}://{{domain}}/{{path}}"),
                ],
                Expected =
                [
                    new Key("protocol", "https"),
                    new Key("domain", "example.com"),
                    new Key("path", "api/v1"),
                    new Key("url", "https://example.com/api/v1"),
                ],
            },
            // Case 5: Circular references (should handle gracefully)
            new() { Keys = [new Key("a", "{{b}}"), new Key("b", "{{a}}")] },
            // Case 6: Self reference
            new() { Keys = [new Key("a", "{{a}}")] },
            // Case 7: Mixed content with references
            new()
            {
                Keys =
                [
                    new Key("name", "Alice"),
                    new Key("greeting", "Hello {{name}}, welcome to {{app}}!"),
                    new Key("app", "MyApp"),
                ],
                Expected =
                [
                    new Key("name", "Alice"),
                    new Key("greeting", "Hello Alice, welcome to MyApp!"),
                    new Key("app", "MyApp"),
                ],
            },
            // Case 8: Deep nesting
            new()
            {
                Keys =
                [
                    new Key("a", "start"),
                    new Key("b", "{{a}}-middle"),
                    new Key("c", "{{b}}-end"),
                    new Key("d", "final: {{c}}"),
                ],
                Expected =
                [
                    new Key("a", "start"),
                    new Key("b", "start-middle"),
                    new Key("c", "start-middle-end"),
                    new Key("d", "final: start-middle-end"),
                ],
            },
            // Case 9: Undefined references
            new() { Keys = [new Key("a", "{{undefined_key}}"), new Key("b", "normal value")] },
            // Case 10: Complex real-world example
            new()
            {
                Keys =
                [
                    new Key("env", "production"),
                    new Key("db_host", "db.{{env}}.company.com"),
                    new Key("api_endpoint", "https://api.{{env}}.company.com/v1"),
                    new Key("timeout", "30"),
                    new Key(
                        "config",
                        "Endpoint: {{api_endpoint}}, DB: {{db_host}}, Timeout: {{timeout}}s"
                    ),
                ],
                Expected =
                [
                    new Key("env", "production"),
                    new Key("db_host", "db.production.company.com"),
                    new Key("api_endpoint", "https://api.production.company.com/v1"),
                    new Key("timeout", "30"),
                    new Key(
                        "config",
                        "Endpoint: https://api.production.company.com/v1, DB: db.production.company.com, Timeout: 30s"
                    ),
                ],
            },
            // Case 11: Special characters in values
            new()
            {
                Keys =
                [
                    new Key("special", "value with spaces"),
                    new Key("reference", "Ref: {{special}} and more"),
                ],
                Expected =
                [
                    new Key("special", "value with spaces"),
                    new Key("reference", "Ref: value with spaces and more"),
                ],
            },
            // Case 12: Empty values and references
            new()
            {
                Keys =
                [
                    new Key("empty_key", ""),
                    new Key("ref_to_empty", "{{empty_key}}"),
                    new Key("normal", "value"),
                ],
                Expected =
                [
                    new Key("empty_key", ""),
                    new Key("ref_to_empty", ""),
                    new Key("normal", "value"),
                ],
            },
        ];
    }
}

public class KeyResolutionTest
{
    private ExpectedResolution[] TestDate = ExpectedResolution.TestData();

    [Fact]
    public void ValueWithNoRef()
    {
        var testdata = TestDate[0];
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
        var testdata = TestDate[1];
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
        var testdata = TestDate[2];
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
        var testdata = TestDate[3];
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
        var testdata = TestDate[6];
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
        var testdata = TestDate[7];
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
        var testdata = TestDate[4];
        Key[] data = testdata.Keys.ToArray();

        Assert.Throws<CircularReferenceException<string>>(() => KeyManager.Resolve(data));
    }

    [Fact]
    public void SelfRef()
    {
        var testdata = TestDate[5];
        Key[] data = testdata.Keys.ToArray();

        Assert.Throws<CircularReferenceException<string>>(() => KeyManager.Resolve(data));
    }

    [Fact]
    public void UndefinedRef()
    {
        var testdata = TestDate[8];
        Key[] data = testdata.Keys.ToArray();

        Assert.Throws<MissingKeyException>(() => KeyManager.Resolve(data));
    }

    [Fact]
    public void Complex1()
    {
        var testdata = TestDate[9];
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
        var testdata = TestDate[10];
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
        var testdata = TestDate[11];
        Key[] data = testdata.Keys.ToArray();
        KeyManager.Resolve(data);

        Assert.Equal<Key>(
            testdata.Expected,
            data,
            (a, b) => a.Name == b.Name && a.Value == b.Value
        );
    }
}
