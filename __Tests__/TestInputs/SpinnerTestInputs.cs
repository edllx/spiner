using spinner;

namespace __Tests__;

public partial class TestInputs
{
    public static IEnumerable<object[]> SpinnerFilesTests =
    [
        new object[]
        {
            new App($"run {Directory.GetCurrentDirectory()}/Files/emptyElements.xml"),
            new App() { },
        },
        new object[]
        {
            new App(
                $"run {Directory.GetCurrentDirectory()}/Files/ServiceTemplate/SimpleService.xml"
            ),
            new App()
            {
                ServiceManager = new()
                {
                    Templates =
                    [
                        new(
                            name: "db",
                            image: "potgress:17",
                            scope: new([
                                new("POSTGRES_USER", "spiner"),
                                new("POSTGRES_PASSWORD", "{{Generated}}"),
                                new("POSTGRES_DB", "{{Generated}}"),
                                new(
                                    "DB_CONNECTION_STRING",
                                    "Server={{CONTAINER_NAME}};Port=5432;Database={{POSTGRES_DB}};User ID={{POSTGRES_USER}};Password={{POSTGRES_PASSWORD}};"
                                ),
                            ]),
                            layers:
                            [
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
                                        new Run(
                                            "psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"
                                        ),
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
                                        new Run(
                                            "psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"
                                        ),
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
                                        new Run(
                                            "psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"
                                        ),
                                        new Run(
                                            "psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"
                                        ),
                                        new Run("echo multiline command echo multiline command"),
                                    ]
                                ),
                            ]
                        ),
                        new(
                            name: "api",
                            buildPath: "./API.Dockerfile",
                            scope: new([new("DB_CONNECTION_STRING", "")])
                        ),
                    ],
                },
            },
        },
        new object[]
        {
            new App(
                $"run {Directory.GetCurrentDirectory()}/Files/RequestTemplate/simpleRequests.xml"
            ),
            new App()
            {
                RequestsManager = new()
                {
                    Templates =
                    [
                        new(name: "getall", method: "GET", path: "weather"),
                        new(
                            name: "get",
                            method: "GET",
                            path: "weather/{{id}}",
                            scope: new([new("id", "")])
                        ),
                        new(
                            name: "add",
                            method: "POST",
                            path: "weather/add",
                            scope: new([new("temperature", ""), new("type", "")]),
                            body: new(
                                type: "json",
                                keys:
                                [
                                    new("temperature", "{{temperature}}"),
                                    new("type", "{{type}}"),
                                ]
                            )
                        ),
                        new(
                            name: "patch",
                            method: "PATCH",
                            path: "weather/{{id}}",
                            scope: new([new("id", ""), new("temperature", ""), new("type", "")]),
                            body: new(
                                type: "json",
                                keys:
                                [
                                    new("id", "{{id}}"),
                                    new("temperature", "{{temperature}}"),
                                    new("type", "{{type}}"),
                                ]
                            )
                        ),
                    ],
                },
            },
        },
    ];

    public static IEnumerable<object[]> SpinnerFilesTestsThrow = [];
}
