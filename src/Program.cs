using spinner;
using static spinner.Parser;

SpinnerParser parser = new();

string fileContent = """
<Spinner>
  <!--Define the structure of each services-->
  <Services>
    <Service name="db" image="potgress:17">
      <Layer name="base-schema">
        <Sql source="./database/Config/schema.sql"/>
      </Layer>

      <Layer name="fahrenheit10" from="base-schema" >
        <Sql source="./database/Config/fahrenheit10.sql"/>
      </Layer>

      <Layer name="celsius10" from="base-schema">
        <Copy source="./database/Config/celsius10.sql" dest="/scripts"/>
        <Run command="psql -U ${POSTGRES_USER} -f /script/celsius10.sql" />
      </Layer>

      <Layer name="bothfandc" from="fahrenheit10,celsius10" >
        <Run>

          echo multiline command

          echo multiline command

        </Run>
      </Layer>
    </Service>

  </Services>
</Spinner>
""";

SpinnerParser spinner = new();

//var st = "          echo multiline command\n          echo multiline command";

//var r = TextToken.Normalize(st, 0);

var res = spinner.Parse(fileContent);
Console.WriteLine(res.ToString(fileContent));
