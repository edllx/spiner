//using Spectre.Console;

using System.Security.Cryptography;
using System.Text;
using spinner;

App app = new(string.Join(" ", args));

var str1 = """
<Services>
    <ServiceTemplate name="db" image="postgress:17">
        <Keys>
            POSTGRES_USER: "spiner"
            POSTGRES_PASSWORD: "Generated"
            POSTGRES_DB: "Generated"
            DB_CONNECTION_STRING: "Server={{CONTAINER_NAME}};Port=5432;Database={{POSTGRES_DB}};User ID={{POSTGRES_USER}};Password={{POSTGRES_PASSWORD}};"
        </Keys>
        <Layer name="base-schema">
            <Copy source="./database/Config/schema.sql" dest="/scripts"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"/>
        </Layer>
        <Layer name="fahrenheit10" from="base-schema">
            <Copy source="./database/Config/schema.sql" dest="/scripts"/>
            <Copy source="./database/Config/fahrenheit10.sql" dest="/scripts"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"/>
        </Layer>
        <Layer name="celsius10" from="base-schema">
            <Copy source="./database/Config/schema.sql" dest="/scripts"/>
            <Copy source="./database/Config/celsius10.sql" dest="/scripts"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"/>
        </Layer>
        <Layer name="bothfandc" from="fahrenheit10,celsius10">
            <Copy source="./database/Config/schema.sql" dest="/scripts"/>
            <Copy source="./database/Config/fahrenheit10.sql" dest="/scripts"/>
            <Copy source="./database/Config/celsius10.sql" dest="/scripts"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"/>
            <Run Command="echo multiline command echo multiline command"/>
        </Layer>
    </ServiceTemplate>
    <ServiceTemplate name="api" build="./API.Dockerfile">
        <Keys>
            DB_CONNECTION_STRING: ""
        </Keys>
    </ServiceTemplate>
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
    <TestSuite>
        <Stack>
            <Service name="db" image="postgress:17">
                <Keys>
                    POSTGRES_USER: "spiner"
                    POSTGRES_PASSWORD: "811yzknkh8xjsy9jch8gfkysy8t5le5d"
                    POSTGRES_DB: "DB_gy03ib5"
                    DB_CONNECTION_STRING: "Server=db;Port=5432;Database=DB_gy03ib5;User ID=spiner;Password=811yzknkh8xjsy9jch8gfkysy8t5le5d;"
                    CONTAINER_NAME: "db"
                </Keys>
                <Layer>
                    <Copy source="./database/Config/schema.sql" dest="/scripts"/>
                    <Copy source="./database/Config/fahrenheit10.sql" dest="/scripts"/>
                    <Run Command="psql -U spiner -f /scripts/schema.sql"/>
                    <Run Command="psql -U spiner -f /scripts/fahrenheit10.sql"/>
                </Layer>
            </Service>
            <Service name="api" build="./API.Dockerfile">
                <Keys>
                    DB_CONNECTION_STRING: "Server=db;Port=5432;Database=DB_gy03ib5;User ID=spiner;Password=811yzknkh8xjsy9jch8gfkysy8t5le5d;"
                    CONTAINER_NAME: "api"
                </Keys>
            </Service>
        </Stack>
        <TestSet mode="sync">
            <Keys>
                id: ""
                temperature: "105"
                type: "Celsius"
            </Keys>
            <Test>
                <Request method="GET" path="weather" >
                </Request>
                <Asserts>
                    <AssertEquals actual="{{response['json']#type}}" expected="array" />
                    <AssertEquals actual="{{response['json']#length}}" expected="3" />
                </Assert>
            </Test>
            <Test>
                <Request method="POST" path="weather/add" >
                    <Keys>
                        temperature: ""
                        type: ""
                    </Keys>
                    <Body>
                        temperature: "{{temperature}}"
                        type: "{{type}}"
                    </Body>
                </Request>
                <Response>
                    <Set key="id" value="{{response['json']['id']}}" />
                </Response>
                <Asserts>
                    <AssertEquals actual="{{response['json']#type}}" expected="object" />
                    <AssertEquals actual="{{response['json']['temperatureC']}}" expected="{{temperature}}" />
                </Assert>
            </Test>
            <Test>
                <Request method="GET" path="weather/{{id}}" >
                    <Keys>
                        id: ""
                    </Keys>
                </Request>
                <Asserts>
                    <AssertEquals actual="{{response['json']#type}}" expected="object" />
                    <AssertEquals actual="{{response['json']['temperatureC']}}" expected="{{temperature}}" />
                </Assert>
            </Test>
            <Test>
                <Asserts>
                    <AssertEquals actual="{{response['json']#type}}" expected="object" />
                    <AssertEquals actual="{{response['json']['id']}}" expected="{{id}}" />
                </Assert>
            </Test>
            <Test>
                <Request method="GET" path="weather" >
                </Request>
                <Asserts>
                    <AssertEquals actual="{{response['json']#type}}" expected="array" />
                    <AssertEquals actual="{{response['json']#length}}" expected="4" />
                </Assert>
            </Test>
        </TestSet>
    </TestSuite>
</TestDescription>
""";
var str2 = """
<Services>
    <ServiceTemplate name="db" image="postgress:17">
        <Keys>
            POSTGRES_USER: "spiner"
            POSTGRES_PASSWORD: "Generated"
            POSTGRES_DB: "Generated"
            DB_CONNECTION_STRING: "Server={{CONTAINER_NAME}};Port=5432;Database={{POSTGRES_DB}};User ID={{POSTGRES_USER}};Password={{POSTGRES_PASSWORD}};"
        </Keys>
        <Layer name="base-schema">
            <Copy source="./database/Config/schema.sql" dest="/scripts"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"/>
        </Layer>
        <Layer name="fahrenheit10" from="base-schema">
            <Copy source="./database/Config/schema.sql" dest="/scripts"/>
            <Copy source="./database/Config/fahrenheit10.sql" dest="/scripts"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"/>
        </Layer>
        <Layer name="celsius10" from="base-schema">
            <Copy source="./database/Config/schema.sql" dest="/scripts"/>
            <Copy source="./database/Config/celsius10.sql" dest="/scripts"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"/>
        </Layer>
        <Layer name="bothfandc" from="fahrenheit10,celsius10">
            <Copy source="./database/Config/schema.sql" dest="/scripts"/>
            <Copy source="./database/Config/fahrenheit10.sql" dest="/scripts"/>
            <Copy source="./database/Config/celsius10.sql" dest="/scripts"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"/>
            <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/celsius10.sql"/>
            <Run Command="echo multiline command echo multiline command"/>
        </Layer>
    </ServiceTemplate>
    <ServiceTemplate name="api" build="./API.Dockerfile">
        <Keys>
            DB_CONNECTION_STRING: ""
        </Keys>
    </ServiceTemplate>
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
    <TestSuite>
        <Stack>
            <Service name="db" image="postgress:17">
                <Keys>
                    CONTAINER_NAME: "db"
                    POSTGRES_USER: "spiner"
                    POSTGRES_PASSWORD: "811yzknkh8xjsy9jch8gfkysy8t5le5d"
                    POSTGRES_DB: "DB_gy03ib5"
                    DB_CONNECTION_STRING: "Server=db;Port=5432;Database=DB_gy03ib5;User ID=spiner;Password=811yzknkh8xjsy9jch8gfkysy8t5le5d;"
                </Keys>
                <Layer>
                    <Copy source="./database/Config/schema.sql" dest="/scripts"/>
                    <Copy source="./database/Config/fahrenheit10.sql" dest="/scripts"/>
                    <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/schema.sql"/>
                    <Run Command="psql -U {{POSTGRES_USER}} -f /scripts/fahrenheit10.sql"/>
                </Layer>
            </Service>
            <Service name="api" image="IMG-api">
                <Keys>
                    DB_CONNECTION_STRING: "Server=db;Port=5432;Database=DB_gy03ib5;User ID=spiner;Password=811yzknkh8xjsy9jch8gfkysy8t5le5d;"
                </Keys>
            </Service>
        </Stack>
        <TestSet mode="sync">
            <Keys>
                id: ""
                temperature: "105"
                type: "Celsius"
            </Keys>
            <Test>
                <Request method="GET" path="weather" />
                <Asserts>
                    <AssertEquals actual="{{response['json']#type}}" expected="array" />
                    <AssertEquals actual="{{response['json']#lenght}}" expected="3" />
                </Assert>
            </Test>
            <Test>
                <Request method="POST" path="weather/add" >
                    <Body>
                        temperature: "105"
                        type: "Celsius"
                    </Body>
                </Request>
                <Response>
                    <Set key="id" value="{{response['json']['id']}}" />
                </Response>
                <Asserts>
                    <AssertEquals actual="{{response['json']#type}}" expected="object" />
                    <AssertEquals actual="{{response['json']['temperatureC']}}" expected="105" />
                </Assert>
            </Test>
            <Test>
                <Request method="GET" path="weather/{{id}}" />
                <Asserts>
                    <AssertEquals actual="{{response['json']#type}}" expected="object" />
                    <AssertEquals actual="{{response['json']['temperatureC']}}" expected="105" />
                </Assert>
            </Test>
            <Test>
                <Request method="PATCH" path="weather" >
                    <Body>
                        id: ""
                        temperature: "50"
                        type: "Celsius"
                    </Body>
                </Request>
                <Asserts>
                    <AssertEquals actual="{{response['json']#type}}" expected="object" />
                    <AssertEquals actual="{{response['json']['id']}}" expected="{{id}}" />
                </Assert>
            </Test>
            <Test>
                <Request method="GET" path="weather" />
                <Asserts>
                    <AssertEquals actual="{{response['json']#type}}" expected="array" />
                    <AssertEquals actual="{{response['json']#lenght}}" expected="4" />
                </Assert>
            </Test>
        </TestSet>
    </TestSuite>
</TestDescription>
""";

var diff = Diff.TextDiff(str1, str2);

Console.WriteLine($"{diff}");

try
{
    //app.Init();
    //Console.WriteLine(app.ToString(0));
    //Console.WriteLine(Tools.StingDiff(str1, str2));
    //Console.WriteLine($"{stableHash} : {invariantHash} : {result} : {results}");
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
