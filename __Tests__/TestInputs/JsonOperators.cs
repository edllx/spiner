using spinner;

namespace __Tests__;

public partial class TestInputs
{
    public static IEnumerable<object[]> JsonOperators =
    [
        // MemberAccess
        new object[]
        {
            "['name']",
            new JsonOperatorToken() { Value = new(2, 4), Type = JsonOperatorType.MemberAccess },
        },
        new object[]
        {
            "[ 'name'   ]",
            new JsonOperatorToken() { Value = new(3, 4), Type = JsonOperatorType.MemberAccess },
        },
        // ArrayIndex
        new object[]
        {
            "[0]",
            new JsonOperatorToken() { Value = new(1, 1), Type = JsonOperatorType.ArrayIndex },
        },
        new object[]
        {
            "[25]",
            new JsonOperatorToken() { Value = new(1, 2), Type = JsonOperatorType.ArrayIndex },
        },
        new object[]
        {
            "[   25  ]",
            new JsonOperatorToken() { Value = new(4, 2), Type = JsonOperatorType.ArrayIndex },
        },
        // MetadataAccess
        new object[]
        {
            "#length",
            new JsonOperatorToken() { Value = new(1, 6), Type = JsonOperatorType.MetadataAccess },
        },
        new object[]
        {
            "#type",
            new JsonOperatorToken() { Value = new(1, 4), Type = JsonOperatorType.MetadataAccess },
        },
    ];

    public static IEnumerable<object[]> JsonOperatorsList =
    [
        new object[]
        {
            "['name']",
            new object[]
            {
                new JsonOperatorToken() { Value = new(2, 4), Type = JsonOperatorType.MemberAccess },
            },
        },
        new object[]
        {
            "['name']#type",
            new object[]
            {
                new JsonOperatorToken() { Value = new(2, 4), Type = JsonOperatorType.MemberAccess },
                new JsonOperatorToken()
                {
                    Value = new(9, 4),
                    Type = JsonOperatorType.MetadataAccess,
                },
            },
        },
        new object[]
        {
            "['name'][20]",
            new object[]
            {
                new JsonOperatorToken() { Value = new(2, 4), Type = JsonOperatorType.MemberAccess },
                new JsonOperatorToken() { Value = new(9, 2), Type = JsonOperatorType.ArrayIndex },
            },
        },
        new object[]
        {
            "[20]['hello']",
            new object[]
            {
                new JsonOperatorToken() { Value = new(1, 2), Type = JsonOperatorType.ArrayIndex },
                new JsonOperatorToken() { Value = new(6, 5), Type = JsonOperatorType.MemberAccess },
            },
        },
        new object[]
        {
            "[20]['hello']#length",
            new object[]
            {
                new JsonOperatorToken() { Value = new(1, 2), Type = JsonOperatorType.ArrayIndex },
                new JsonOperatorToken() { Value = new(6, 5), Type = JsonOperatorType.MemberAccess },
                new JsonOperatorToken()
                {
                    Value = new(14, 6),
                    Type = JsonOperatorType.MetadataAccess,
                },
            },
        },
        new object[]
        {
            "['test'][100]#length",
            new object[]
            {
                new JsonOperatorToken() { Value = new(2, 4), Type = JsonOperatorType.MemberAccess },
                new JsonOperatorToken() { Value = new(9, 3), Type = JsonOperatorType.ArrayIndex },
                new JsonOperatorToken()
                {
                    Value = new(14, 6),
                    Type = JsonOperatorType.MetadataAccess,
                },
            },
        },
    ];
}
