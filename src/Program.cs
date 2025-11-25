using System.Text.Json;
using spinner;
using static spinner.Parser;

SpinnerParser parser = new();

string fileContent = """
<Spinner>
  <!--Define the structure of each services-->
  <Services>
    <Service name="db" image="potgress:17">
      <Key name="POSTGRES_USER" value="spiner" />
      <GeneratedKey name="POSTGRES_PASSWORD" len="32" />
      <GeneratedKey name="POSTGRES_DB" len="10" />
      <Key name="DB_CONNECTION_STRING" value="Server={{CONTAINER_NAME}};Port=5432;Database={{POSTGRES_DB}};User ID={{POSTGRES_USER}};Password={{POSTGRES_PASSWORD}};"/>

      <Layer name="base-schema" >
        <Sql source="./database/Config/schema.sql"/>
      </Layer>

      <Layer name="fahrenheit10" from="base-schema" >
        <Sql source="./database/Config/fahrenheit10.sql"/>
      </Layer>

      <Layer name="celsius10" from="base-schema">
        <Copy source="./database/Config/celsius10.sql" dest="/scripts"/>
        <Run command="psql -U {{POSTGRES_USER}} -f /script/celsius10.sql" />
      </Layer>

      <Layer name="bothfandc" from="fahrenheit10,celsius10" >
        <Run>
          echo multiline command
          echo multiline command
        </Run>
      </Layer>
    </Service  >

    <Service name="api" build="./API.Dockerfile">
      <Key name="DB_CONNECTION_STRING"/>
    </Service>
  </Services>

  <!--Define the structure of each request-->
  <Requests>
    <Request name="getall" method="GET" path="weather"/>

    <Request name="get" method="GET" path="weather/{{id}}">
      <Key name="id"/>
    </Request>

    <Request name="add" method="POST" path="weather/add">
      <Key name="temperature"/>
      <Key name="type"/>

      <Body type="json">
        <Key name="temperature" value="{{temperature}}"/>
        <Key name="type" value="{{type}}"/>
      </Body>

    </Request>

    <Request name="patch" method="PATCH" path="weather/{{id}}">
      <Key name="id"/>
      <Key name="temperature"/>
      <Key name="type"/>

      <Body type="json">
        <Key name="temperature" value="{{temperature}}"/>
        <Key name="type" value="{{type}}"/>
      </Body>
    </Request>
  </Requests>

  <TestSuite>
    <Stack>
      <Service name="db" layer="fahrenheit10"/>
      <Service name="api">
        <Arg name="DB_CONNECTION_STRING" from="db" key="DB_CONNECTION_STRING"/>
      </Service>
    </Stack>

    <Tests mode="sync">
      <Key name="id" />
      <Test>
        <Request name="getall"/>
        <Asserts>
          <!-- Response is a contextual variable to each test-->
          <NotNull key="Response.Content"/>
          <Equals actual="{{Response.Content.Type}}" expected="Array"/>
          <Equals actual="{{Response.Content.Len}}" expected="3"/>
        </Asserts>
      </Test>

      <Test>
        <Key name="temperature" value="30"/>
        <Key name="type" value="Celsius"/>

        <Request name="add">
          <Arg name="temperature" value="{{temperature}}"/>
          <Arg name="temperature" value="{{type}}"/>
        </Request>

        <Response>
          <Set key="id" value="{{Response.Content.id}"/>
        </Response>

        <Asserts>
          <NotNull key="Response.Content"/>
          <Equals actual="{{Response.Content.Type}}" expected="Object"/>
          <Equals actual="{{Response.Content.temperatureC}}" expected="{{temperature}}"/>
        </Asserts>
      </Test>

      <Test>
        <Request name="getall"/>
        <Asserts>
          <NotNull key="Response.Content"/>
          <Equals actual="{{Response.Content.Type}}" expected="Array"/>
          <Equals actual="{{Response.Content.Len}}" expected="4"/>
        </Asserts>
      </Test>
    </Tests>
  </TestSuite>

</Spinner>
""";

SpinnerParser spinner = new();

PodmanService service = new();

var serviceName = "spinner-db-test";
var dbName = "postgres";
var userName = "postgres";
var password = "postgres";
var inPort = 5432;
var outPort = 5432;

var buildPath = "/home/etienne/Desktop/repository/demo/tya/Dockerfile";
var contexPath = "/home/etienne/Desktop/repository/demo/tya";
var image = "meteo-ap";

//await service.BuildImageAsync(buildPath, contexPath, image);

/*
await service.RunContainerAsync(
    "postgres:17",
    serviceName,
    envVariables:
    [
        ("POSTGRES_USER", userName),
        ("POSTGRES_DB", dbName),
        ("POSTGRES_PASSWORD", password),
    ],
    ports: [(inPort, outPort)],
    replace: true
);

await service.ExecCommandAsync(
    serviceName,
    $"bash -c \"while ! pg_isready -U {userName}; do sleep 1; done\""
);

await service.ExecCommandAsync(
    serviceName,
    $"psql -U {userName} -d {dbName} -c \"CREATE TABLE users (id VARCHAR(100), name VARCHAR(100));\""
);
await service.ExecCommandAsync(
    serviceName,
    $"psql -U {userName} -d {dbName} -c \"INSERT INTO users(id,name)VALUES('user-2','jhon')\""
);
await service.ExecCommandAsync(
    serviceName,
    $"psql -U {userName} -d {dbName} -c \"SELECT * FROM users\""
);

await service.RemoveContainerAsync(serviceName, force: true);


//Console.WriteLine(Directory.GetCurrentDirectory());



var json = new
{
    person = new
    {
        name = "jhon",
        age = 30,
        adress = new { steet = "123 Main St", city = "New York" },
    },
    hobbies = new List<string>() { "reading", "gaming", "coding" },
};

var jsonContent = JsonSerializer.Serialize(new { });

var el = """
{
  "person": {
    "name": "jhon",
    "age": 30,
    "adress": {
      "steet": "123 Main St",
      "city": "New York"
    }
  },
  "hobbies": [
    "reading",
    "gaming",
    "coding"
  ]
}
""";

using JsonDocument doc = JsonDocument.Parse(el);
JsonElement element = doc.RootElement;
Console.WriteLine(element.GetProperty("person").ToString());
Console.WriteLine(jsonContent.ToString());
$["mixed.keys"][3]#len
*/

//HttpContextOptions options = new() { BaseUri = "http://localhost:5353" };

//HttpContext context = new(options);

//var res = await context.Get("weather");

/*
var text = File.ReadAllText(
    "/home/etienne/Desktop/repository/edllx/spiner/__Tests__/Files/GenericElements.xml"
);

*/

//Console.WriteLine(res.FindProperty("date.day", 0));
//Console.WriteLine(res.FindProperty("temperatureC", 1));
//Console.WriteLine(res.FindProperty("temperatureF", 0));
//Console.WriteLine(res.FindProperty("temperatureC", 0));

// Response.Json#Type
// Response.Json#Length
// Response.Json#Length

//var st = " <!--Define the structure of each request-->";

/*
var st = """
<!--Define the structure of each request-->
""";
*/

//var res = p.Parse(new ParseContext(st));

//var res = spinner.Parse(fileContent);
//Console.WriteLine(res.ToString(fileContent));
//Console.WriteLine(XML.GenericElement.Parse(new(st)).ToString(st));
var st = "\"test \"";
var pa = StringLiteral.Parse(new(st));

Console.WriteLine(pa.ToString(st));
