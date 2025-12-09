//using Spectre.Console;

using System.Security.Cryptography;
using System.Text;
using spinner;

App app = new(string.Join(" ", args));

var str1 = """
<Services>

</Services>

<Requests>
    <RequestTemplate name="getall" method="GET" path="weather">
    </RequestTemplate>
    <RequestTemplate name="get" method="GET" path="weather/{{id}}">
        <Keys>
            id: ""
        </Keys>
    </RequestTemplate>
    <RequestTemplate name="add" method="POST" path="weather/add">
        <Keys>
            temperature: ""
            type: ""
        </Keys>
        <Body>
            temperature: "{{temperature}}"
            type: "{{type}}"
        </Body>
    </RequestTemplate>
    <RequestTemplate name="patch" method="PATCH" path="weather">
        <Keys>
            id: ""
            temperature: ""
            type: ""
        </Keys>
        <Body>
            id: "{{id}}"
            temperature: "{{temperature}}"
            type: "{{type}}"
        </Body>
    </RequestTemplate>
</Requests>

<TestDescription>

</TestDescription>
""";
var str2 = """
<Services>

</Services>

<Requests>
    <RequestTemplate name="getall" method="GET" path="weather">
    </RequestTemplate>
    <RequestTemplate name="get" method="GET" path="weather/{{id}}">
        <Keys>
            id: ""
        </Keys>
    </RequestTemplate>
    <RequestTemplate name="add" method="POST" path="weather/add">
        <Keys>
            temperature: ""
            type: ""
        </Keys>
        <Body>
            temperature: "{{temperature}}"
            type: "{{type}}"
        </Body>
    </RequestTemplate>

    <RequestTemplate name="patch" method="PATCH" path="weathers">
        <Keys>
            id: ""
            temperature: ""
            type: ""
        </Keys>
        <Body>
            id: "{{id}}"
            temperature: "{{temperature}}"
            type: "{{type}}"
        </Body>
    </RequestTemplate>
</Requests>

<TestDescription>

</TestDescription>
""";

var diff = Diff.TextDiff(str1, str2);

Console.WriteLine($"{diff}");

try
{
    app.Init();
    //Console.WriteLine(app.ToString(0));
    //Console.WriteLine(Tools.StingDiff(str1, str2));
    //Console.WriteLine($"{stableHash} : {invariantHash} : {result} : {results}");
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
