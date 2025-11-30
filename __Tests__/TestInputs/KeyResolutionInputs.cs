using spinner;

namespace __Tests__;

public class ExpectedResolution
{
    public Key[] Keys { get; init; } = [];
    public Key[] Expected { get; set; } = [];
}

public partial class TestInputs
{
    public static IEnumerable<object[]> KeyResolutionInput =
    [
        // Case 1: Simple values without references
        new object[]
        {
            new Key[]
            {
                new Key("name", "John"),
                new Key("age", "30"),
                new Key("city", "New York"),
            },
            new Key[]
            {
                new Key("name", "John"),
                new Key("age", "30"),
                new Key("city", "New York"),
            },
        },
        // Case 2: Single level references
        new object[]
        {
            new Key[]
            {
                new Key("first_name", "John"),
                new Key("last_name", "Doe"),
                new Key("full_name", "{{first_name}} {{last_name}}"),
            },
            new Key[]
            {
                new Key("first_name", "John"),
                new Key("last_name", "Doe"),
                new Key("full_name", "John Doe"),
            },
        },
        // Case 3: Nested references
        new object[]
        {
            new Key[]
            {
                new Key("a", "Hello"),
                new Key("b", "{{a}} World"),
                new Key("c", "{{b}}!!!"),
            },
            new Key[]
            {
                new Key("a", "Hello"),
                new Key("b", "Hello World"),
                new Key("c", "Hello World!!!"),
            },
        },
        // Case 4: Multiple references in one value

        new object[]
        {
            new Key[]
            {
                new Key("protocol", "https"),
                new Key("domain", "example.com"),
                new Key("path", "api/v1"),
                new Key("url", "{{protocol}}://{{domain}}/{{path}}"),
            },
            new Key[]
            {
                new Key("protocol", "https"),
                new Key("domain", "example.com"),
                new Key("path", "api/v1"),
                new Key("url", "https://example.com/api/v1"),
            },
        },
        // Case 5: Circular references (should handle gracefully)

        new object[]
        {
            new Key[]
            {
                new Key("name", "Alice"),
                new Key("greeting", "Hello {{name}}, welcome to {{app}}!"),
                new Key("app", "MyApp"),
            },
            new Key[]
            {
                new Key("name", "Alice"),
                new Key("greeting", "Hello Alice, welcome to MyApp!"),
                new Key("app", "MyApp"),
            },
        },
        // Case 8: Deep nesting
        new object[]
        {
            new Key[]
            {
                new Key("a", "start"),
                new Key("b", "{{a}}-middle"),
                new Key("c", "{{b}}-end"),
                new Key("d", "final: {{c}}"),
            },
            new Key[]
            {
                new Key("a", "start"),
                new Key("b", "start-middle"),
                new Key("c", "start-middle-end"),
                new Key("d", "final: start-middle-end"),
            },
        },
        // Case 9: Undefined references
        // Case 10: Complex real-world example
        new object[]
        {
            new Key[]
            {
                new Key("env", "production"),
                new Key("db_host", "db.{{env}}.company.com"),
                new Key("api_endpoint", "https://api.{{env}}.company.com/v1"),
                new Key("timeout", "30"),
                new Key(
                    "config",
                    "Endpoint: {{api_endpoint}}, DB: {{db_host}}, Timeout: {{timeout}}s"
                ),
            },
            new Key[]
            {
                new Key("env", "production"),
                new Key("db_host", "db.production.company.com"),
                new Key("api_endpoint", "https://api.production.company.com/v1"),
                new Key("timeout", "30"),
                new Key(
                    "config",
                    "Endpoint: https://api.production.company.com/v1, DB: db.production.company.com, Timeout: 30s"
                ),
            },
        },
        // Case 11: Special characters in values
        new object[]
        {
            new Key[]
            {
                new Key("special", "value with spaces"),
                new Key("reference", "Ref: {{special}} and more"),
            },
            new Key[]
            {
                new Key("special", "value with spaces"),
                new Key("reference", "Ref: value with spaces and more"),
            },
        },
        // Case 12: Empty values and references
        new object[]
        {
            new Key[]
            {
                new Key("empty_key", ""),
                new Key("ref_to_empty", "{{empty_key}}"),
                new Key("normal", "value"),
            },
            new Key[]
            {
                new Key("empty_key", ""),
                new Key("ref_to_empty", ""),
                new Key("normal", "value"),
            },
        },
    ];

    public static IEnumerable<object[]> KeyResolutionInputShoulThrow =
    [
        new object[]
        {
            new Key[] { new Key("a", "{{undefined_key}}"), new Key("b", "normal value") },
            new MissingKeyException("undefined_key"),
        },
        new object[]
        {
            new Key[] { new Key("a", "{{b}}"), new Key("b", "{{a}}") },
            new CircularReferenceException<string>(["a", "b"]),
        },
        new object[]
        {
            new Key[] { new Key("a", "{{a}}") },
            new CircularReferenceException<string>(["a"]),
        },
    ];
}
