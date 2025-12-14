using spinner;

namespace __Tests__;

public partial class TestInputs
{
    private static List<ServiceTemplate> _serviceTemplates1 =
    [
        new(
            name: "db",
            image: "postgres:17",
            scope: new([
                new("POSTGRES_USER", "spiner"),
                new("POSTGRES_PASSWORD", "Generated"),
                new("POSTGRES_DB", "Generated"),
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
            ]
        ),
        new(
            name: "api",
            buildPath: "./API.Dockerfile",
            scope: new([new("DB_CONNECTION_STRING", "")])
        ),
    ];

    private static List<RequestTemplate> _requestTemplates1 =
    [
        new(name: "getall", method: "GET", path: "weather"),
        new(name: "get", method: "GET", path: "weather/{{id}}", scope: new([new("id", "")])),
        new(
            name: "add",
            method: "POST",
            path: "weather/add",
            scope: new([new("temperature", ""), new("type", "")]),
            body: new(
                type: "json",
                keys: [new("temperature", "{{temperature}}"), new("type", "{{type}}")]
            )
        ),
        new(
            name: "patch",
            method: "PATCH",
            path: "weather",
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
    ];

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
            new App() { ServiceManager = new() { Templates = _serviceTemplates1 } },
        },
        new object[]
        {
            new App(
                $"run {Directory.GetCurrentDirectory()}/Files/RequestTemplate/simpleRequests.xml"
            ),
            new App() { RequestsManager = new() { Templates = _requestTemplates1 } },
        },
        new object[]
        {
            new App($"run {Directory.GetCurrentDirectory()}/Files/full.xml"),
            new App()
            {
                ServiceManager = new() { Templates = _serviceTemplates1 },
                RequestsManager = new() { Templates = _requestTemplates1 },
                TestManager = new()
                {
                    Tests =
                    [
                        new(
                            new Stack([
                                new(
                                    name: "db",
                                    image: "postgres:17",
                                    scope: new([
                                        new("POSTGRES_USER", "spiner"),
                                        new(
                                            "POSTGRES_PASSWORD",
                                            $"{Tools.GenerateRandomString(32, seed: 10)}"
                                        ),
                                        new(
                                            "POSTGRES_DB",
                                            $"{Tools.GenerateRandomString(10, seed: 20, prefix: "DB_")}"
                                        ),
                                        new(
                                            "DB_CONNECTION_STRING",
                                            $"Server=localhost;Port=5432;Database={Tools.GenerateRandomString(10, seed: 20, prefix: "DB_")};User ID=spiner;Password={Tools.GenerateRandomString(32, seed: 10)};"
                                        ),
                                        // auto generated
                                        new("CONTAINER_NAME", "localhost"),
                                    ]),
                                    commands:
                                    [
                                        new Copy("./database/Config/schema.sql", "/scripts"),
                                        new Copy("./database/Config/fahrenheit10.sql", "/scripts"),
                                        new Run("psql -U spiner -f /scripts/schema.sql"),
                                        new Run("psql -U spiner -f /scripts/fahrenheit10.sql"),
                                    ]
                                ),
                                new(
                                    name: "api",
                                    image: "sp-img-api",
                                    target: true,
                                    scope: new([
                                        new(
                                            "DB_CONNECTION_STRING",
                                            $"Server=localhost;Port=5432;Database={Tools.GenerateRandomString(10, seed: 20, prefix: "DB_")};User ID=spiner;Password={Tools.GenerateRandomString(32, seed: 10)};"
                                        ),
                                        new("CONTAINER_NAME", "localhost"),
                                    ]),
                                    commands: []
                                ),
                            ]),
                            [
                                new(
                                    mode: "sync",
                                    scope: new([
                                        new("id", "random-id"),
                                        new("temperature", "105"),
                                        new("type", "Celsius"),
                                    ]),
                                    testSet:
                                    [
                                        new(
                                            scope: new(),
                                            request: new("getall", path: "weather", method: "GET"),
                                            response: null,
                                            asserts: new([
                                                new AssertEquals(
                                                    "array",
                                                    "{{response['json']#type}}"
                                                ),
                                                new AssertEquals(
                                                    "3",
                                                    "{{response['json']#length}}"
                                                ),
                                            ])
                                        ),
                                        new(
                                            scope: new([
                                                new("temperature", "105"),
                                                new("type", "Celsius"),
                                            ]),
                                            request: new(
                                                "add",
                                                path: "weather/add",
                                                method: "POST",
                                                body: new(
                                                    "json",
                                                    keys:
                                                    [
                                                        new("temperature", "105"),
                                                        new("type", "Celsius"),
                                                    ]
                                                )
                                            ),
                                            response: new([
                                                new("id", "{{response['json']['id']}}"),
                                            ]),
                                            asserts: new([
                                                new AssertEquals(
                                                    "object",
                                                    "{{response['json']#type}}"
                                                ),
                                                new AssertEquals(
                                                    "105",
                                                    "{{response['json']['temperatureC']}}"
                                                ),
                                            ])
                                        ),
                                        new(
                                            scope: new([new("id", "random-id")]),
                                            request: new(
                                                "get",
                                                path: "weather/random-id",
                                                method: "GET"
                                            ),
                                            asserts: new([
                                                new AssertEquals(
                                                    "object",
                                                    "{{response['json']#type}}"
                                                ),
                                                new AssertEquals(
                                                    "105",
                                                    "{{response['json']['temperatureC']}}"
                                                ),
                                            ])
                                        ),
                                        new(
                                            scope: new([
                                                new("id", "random-id"),
                                                new("temperature", "50"),
                                                new("type", "Celsius"),
                                            ]),
                                            request: new(
                                                "patch",
                                                path: "weather",
                                                method: "PATCH",
                                                body: new(
                                                    "json",
                                                    keys:
                                                    [
                                                        new("id", "random-id"),
                                                        new("temperature", "50"),
                                                        new("type", "Celsius"),
                                                    ]
                                                )
                                            ),
                                            asserts: new([
                                                new AssertEquals(
                                                    "object",
                                                    "{{response['json']#type}}"
                                                ),
                                                new AssertEquals(
                                                    "random-id",
                                                    "{{response['json']['id']}}"
                                                ),
                                            ])
                                        ),
                                        new(
                                            scope: new(),
                                            request: new("getall", path: "weather", method: "GET"),
                                            response: null,
                                            asserts: new([
                                                new AssertEquals(
                                                    "array",
                                                    "{{response['json']#type}}"
                                                ),
                                                new AssertEquals(
                                                    "4",
                                                    "{{response['json']#length}}"
                                                ),
                                            ])
                                        ),
                                    ]
                                ),
                            ]
                        ),
                    ],
                },
            },
        },
    ];

    public static IEnumerable<object[]> SpinnerFilesTestsThrow = [];
}
