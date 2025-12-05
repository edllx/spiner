using spinner;

namespace __Tests__;

public partial class TestInputs
{
    public static IEnumerable<object[]> LayerTests =
    [
        new object[]
        {
            new Layer[]
            {
                new(
                    "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
                new(
                    "fahrenheit10",
                    from: "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                    ]
                ),
                new(
                    "celsius10",
                    from: "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/celsius10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"),
                    ]
                ),
                new(
                    "bothfandc",
                    from: "fahrenheit10,celsius10",
                    commands: [new Run("echo multiline command echo multiline command")]
                ),
            },
            new Layer[]
            {
                new(
                    "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
                new(
                    "fahrenheit10",
                    from: "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                    ]
                ),
                new(
                    "celsius10",
                    from: "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Copy("./database/Config/celsius10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"),
                    ]
                ),
                new(
                    "bothfandc",
                    from: "fahrenheit10,celsius10",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Copy("./database/Config/celsius10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"),
                        new Run("echo multiline command echo multiline command"),
                    ]
                ),
            },
        },
        new object[]
        {
            new Layer[]
            {
                new(
                    "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
                new(
                    "fahrenheit10",
                    from: "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                    ]
                ),
                new(
                    "celsius10",
                    from: "base-schema,fahrenheit10",
                    commands:
                    [
                        new Copy("./database/Config/celsius10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"),
                    ]
                ),
                new(
                    "bothfandc",
                    from: "fahrenheit10,celsius10",
                    commands: [new Run("echo multiline command echo multiline command")]
                ),
            },
            new Layer[]
            {
                new(
                    "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
                new(
                    "fahrenheit10",
                    from: "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                    ]
                ),
                new(
                    "celsius10",
                    from: "base-schema,fahrenheit10",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Copy("./database/Config/celsius10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"),
                    ]
                ),
                new(
                    "bothfandc",
                    from: "fahrenheit10,celsius10",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Copy("./database/Config/celsius10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"),
                        new Run("echo multiline command echo multiline command"),
                    ]
                ),
            },
        },
        new object[]
        {
            new Layer[]
            {
                new(
                    "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
                new(
                    "fahrenheit10",
                    from: "unknown-layer",
                    commands:
                    [
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                    ]
                ),
            },
            new Layer[]
            {
                new(
                    "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
                new(
                    "fahrenheit10",
                    from: "unknown-layer",
                    commands:
                    [
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                    ]
                ),
            },
        },
        new object[]
        {
            new Layer[]
            {
                new(
                    "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
                new(
                    "fahrenheit10",
                    from: "unknown-layer,base-schema",
                    commands:
                    [
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                    ]
                ),
            },
            new Layer[]
            {
                new(
                    "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
                new(
                    "fahrenheit10",
                    from: "unknown-layer,base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                    ]
                ),
            },
        },
        new object[]
        {
            new Layer[]
            {
                new(
                    "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
                new(
                    "fahrenheit10",
                    from: "base-schema,base-schema",
                    commands:
                    [
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                    ]
                ),
            },
            new Layer[]
            {
                new(
                    "base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
                new(
                    "fahrenheit10",
                    from: "base-schema,base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"),
                    ]
                ),
            },
        },
        new object[]
        {
            new Layer[]
            {
                new(
                    "base-schema",
                    from: "base-schema,base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
            },
            new Layer[]
            {
                new(
                    "base-schema",
                    from: "base-schema,base-schema",
                    commands:
                    [
                        new Copy("./database/Config/schema.sql", "/scripts"),
                        new Run("psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"),
                    ]
                ),
            },
        },
    ];
}
