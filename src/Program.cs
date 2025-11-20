using spinner;
using static spinner.Parser;

SpinnerParser parser = new();

string fileContent = """
<Spinner>
  <!--Define the structure of each services-->

  <!--Key values are resolved when the element is instantiated wich let the posibility 
  to compose key based on other keys-->

  <!--Param are key that are suposed to be provided when the element is instantiated
  by definition they are not resolved-->
  <Services>
    <Service name="db" image="potgress:17">
      <Key name="POSTGRES_USER" value="spiner" />
      <GeneratedKey name="POSTGRES_PASSWORD" len="32" />
      <!--Service comment-->
      <GeneratedKey name="POSTGRES_DB" len="10" />
      <Key name="DB_CONNECTION_STRING" value="Server=${CONTAINER_NAME};Port=5432;Database=${POSTGRES_DB};User ID=${POSTGRES_USER};Password=${POSTGRES_PASSWORD};"/>
    </Service>

    <Service name="api" build="./API.Dockerfile">
      <Key name="DB_CONNECTION_STRING"/>
    </Service>
  </Services>
</Spinner>
""";

SpinnerParser spinner = new();

var res = spinner.Parse(fileContent);

Console.WriteLine(res.ToString(fileContent));
