using System.Text.Json;
using spinner;

namespace __Tests__;

//[Collection("test-inputs")]
public class JsonParsingTest
{
    public static IEnumerable<object[]> TestData => TestInputs.JsonInput;
    public static IEnumerable<object[]> JsonReponse => TestInputs.JsonResponseInput;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestName(string json, JsonValue[] expecteds)
    {
        var document = JsonDocument.Parse(json);
        var response = new HttpResponse() { Document = document };

        for (int i = 0; i < expecteds.Length; i++)
        {
            var el = expecteds[i];
            var expected = el.Value;
            var actual = response.JsonFind(el.Path);

            Assert.True(
                el.Value == actual.Value,
                $"{json}\nPath: {el.Path}\nExpected: {el.Value}\nActual: {actual.Value}"
            );
        }
    }

    [Theory]
    [MemberData(nameof(JsonReponse))]
    public void TestJsonResponse(string json, Scope scope, JsonResponse[] expecteds)
    {
        var document = JsonDocument.Parse(json);
        var response = new HttpResponse() { Document = document };

        for (int i = 0; i < expecteds.Length; i++)
        {
            JsonResponse el = expecteds[i];
            string expected = el.ToString();
            var actual = response.JsonFind(el.Path, scope).ToString();

            Assert.True(
                expected == actual,
                $"{json}\n{scope.ToString(0)}\n{Tools.StringDiff(expected, actual)}"
            );
        }
    }
}

public class JsonOperatorTests
{
    public static IEnumerable<object[]> TestData => TestInputs.JsonOperators;
    public static IEnumerable<object[]> TestData2 => TestInputs.JsonOperatorsList;
    private static JsonOperatorParser Parser = new();

    [Theory]
    [MemberData(nameof(TestData))]
    public void ParsingTest(string op, JsonOperatorToken token)
    {
        var result = Parser.Parse(new ParseContext(op));
        Assert.True(result.Success);
        var tok = (JsonOperatorToken)result.Token;
        var expected = token.Value.ToString(op);
        var actual = tok.Value.ToString(op);
        Assert.True(expected == actual, $"{op}\nExpected: {expected}\nActual: {actual}");
        Assert.True(
            token.Type == tok.Type,
            $"{op}\nExpected: {token.Type.ToString()}\nActual: {tok.Type.ToString()}"
        );
    }

    [Theory]
    [MemberData(nameof(TestData2))]
    public void OperatorsGroup(string ops, JsonOperatorToken[] tokens)
    {
        var result = JsonParser.ExtractOperators(ops);

        Assert.True(result.Length == tokens.Length, "Token number missmatch");

        for (int i = 0; i < result.Length; i++)
        {
            var expected = tokens[i];
            var actual = result[i];

            Assert.True(
                expected.Type == actual.Type,
                $"Path: {ops}\nExpected: {expected.Type.ToString()} > [{i}]\nActual: {actual.Type.ToString()}"
            );

            Assert.True(
                expected.Value.ToString(ops) == actual.Value.ToString(ops),
                $"Path: {ops}\nExpected: {expected.Value.ToString(ops)} > [{i}]\nActual: {actual.Value.ToString(ops)}"
            );
        }
    }
}
