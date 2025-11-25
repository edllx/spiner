using spinner;

namespace __Tests__;

public class ExpectedJsonResolution
{
    public required string Json { get; init; }
    public required spinner.JsonValue[] Expected { get; init; }
}

public partial class TestInputs
{
    public static IEnumerable<object[]> JsonInput =
    [
        new object[]
        {
            """{"user": {"name": "John", "age": 30}}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['user']['name']",
                    Value = "John",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['user']['age']",
                    Value = "30",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['user']#type",
                    Value = "object",
                },
            },
        },
        new object[]
        {
            """{"items": ["apple", "banana", "cherry"]}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['items'][0]",
                    Value = "apple",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['items'][2]",
                    Value = "cherry",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['items']#length",
                    Value = "3",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['items']#type",
                    Value = "array",
                },
            },
        },
        new object[]
        {
            """{"users": [{"id": 1, "name": "Alice"}, {"id": 2, "name": "Bob"}]}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['users'][0]['name']",
                    Value = "Alice",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['users'][1]['id']",
                    Value = "2",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['users']#length",
                    Value = "2",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['users'][0]#type",
                    Value = "object",
                },
            },
        },
        new object[]
        {
            """{"matrix": [[1, 2, 3], [4, 5, 6]]}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['matrix'][1][2]",
                    Value = "6",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['matrix'][0]#length",
                    Value = "3",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['matrix']#length",
                    Value = "2",
                },
            },
        },
        new object[]
        {
            """{"name": "John", "active": true, "count": 42, "nullValue": null}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['name']#type",
                    Value = "string",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['name']#length",
                    Value = "4",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['active']#type",
                    Value = "boolean",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['count']#type",
                    Value = "number",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['nullValue']#type",
                    Value = "",
                },
            },
        },
        new object[]
        {
            """{"key.with.dots": "value1", "normal": "value2"}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['key.with.dots']",
                    Value = "value1",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['normal']",
                    Value = "value2",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['key.with.dots']#type",
                    Value = "string",
                },
            },
        },
        new object[]
        {
            """{"key#with#hashes": "hashValue", "key[with]brackets": "bracketValue"}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['key#with#hashes']",
                    Value = "hashValue",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['key[with]brackets']",
                    Value = "bracketValue",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['key#with#hashes']#length",
                    Value = "9",
                },
            },
        },
        new object[]
        {
            """{"mixed.keys": {"a.b": {"c[d]": {"e#f": "final value"}}}}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['mixed.keys']['a.b']['c[d]']['e#f']",
                    Value = "final value",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['mixed.keys']['a.b']#type",
                    Value = "object",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['mixed.keys']['a.b']['c[d]']['e#f']#length",
                    Value = "11",
                },
            },
        },
        new object[]
        {
            """{"store": {"books": [{"title": "Book 1", "metadata": {"pages": 100}}]}}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['store']['books'][0]['title']",
                    Value = "Book 1",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['store']['books'][0]['metadata']['pages']",
                    Value = "100",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['store']['books']#length",
                    Value = "1",
                },
            },
        },
        new object[]
        {
            """{"users": [{"full.name": "John Doe", "age": 30}]}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['users'][0]['full.name']",
                    Value = "John Doe",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['users'][0]['age']",
                    Value = "30",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['users'][0]['full.name']#length",
                    Value = "8",
                },
            },
        },
        new object[]
        {
            """{"data": {"list#items": ["a", "b", "c"]}}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['data']['list#items'][1]",
                    Value = "b",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['data']['list#items']#length",
                    Value = "3",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['data']['list#items']#type",
                    Value = "array",
                },
            },
        },
        new object[]
        {
            """{"special.keys": {"a.b": [1, 2, 3]}}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['special.keys']['a.b']#length",
                    Value = "3",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['special.keys']['a.b'][1]",
                    Value = "2",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['special.keys']#type",
                    Value = "object",
                },
            },
        },
        new object[]
        {
            """{"weird#key": "test", "normalKey": "value"}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['weird#key']",
                    Value = "test",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['weird#key']#type",
                    Value = "string",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['normalKey']",
                    Value = "value",
                },
            },
        },
        new object[]
        {
            """{"empty": "", "nullValue": null, "emptyArray": []}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['empty']#length",
                    Value = "0",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['nullValue']#type",
                    Value = "",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['emptyArray']#length",
                    Value = "0",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['emptyArray']#type",
                    Value = "array",
                },
            },
        },
        new object[]
        {
            """
                {
                    "users": [
                        {
                            "id": 1,
                            "full.name": "Alice Smith",
                            "contact.info": {
                                "email#primary": "alice@test.com",
                                "phones": ["123-4567", "987-6543"]
                            }
                        }
                    ]
                }
                """,
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['users'][0]['contact.info']['email#primary']",
                    Value = "alice@test.com",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['users'][0]['contact.info']['phones']#length",
                    Value = "2",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['users'][0]['contact.info']['phones'][1]",
                    Value = "987-6543",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['users'][0]['full.name']",
                    Value = "Alice Smith",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['users']#length",
                    Value = "1",
                },
            },
        },
        new object[]
        {
            """{"level1": {"level2.key": {"level3[data]": {"level4#value": "deep"}}}}""",
            new[]
            {
                new JsonValue
                {
                    Found = true,
                    Path = "['level1']['level2.key']['level3[data]']['level4#value']",
                    Value = "deep",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['level1']['level2.key']#type",
                    Value = "object",
                },
                new JsonValue
                {
                    Found = true,
                    Path = "['level1']['level2.key']['level3[data]']['level4#value']#length",
                    Value = "4",
                },
            },
        },
        new object[]
        {
            """{"name": "John"}""",
            new[]
            {
                new JsonValue
                {
                    Found = false,
                    Path = "['nonexistent']",
                    Value = "",
                },
                new JsonValue
                {
                    Found = false,
                    Path = "['name'][0]",
                    Value = "",
                }, // Type mismatch
                new JsonValue
                {
                    Found = false,
                    Path = "['invalid']#metadata",
                    Value = "",
                },
            },
        },
        new object[]
        {
            """{"items": ["a", "b"]}""",
            new[]
            {
                new JsonValue
                {
                    Found = false,
                    Path = "['items'][5]",
                    Value = "",
                }, // Out of bounds
                new JsonValue
                {
                    Found = false,
                    Path = "['items']['invalid']",
                    Value = "",
                }, // Invalid property on array
            },
        },
        /*
        */
    ];
}
