# Spinner

> Declarative API integration test orchestration using isolated containerized environments.

Spinner is a test orchestration tool for testing APIs together with their dependent
services.

Instead of requiring an already-running API and database, Spinner describes the
test environment and test scenarios declaratively, then creates the required
containers, executes the tests, evaluates assertions and tears the environment down.

## Why?

Testing an API often requires reproducing its dependencies:

- API
- PostgreSQL database
- database schema and test data
- service configuration
- dependent services

Spinner aims to make these environments reproducible and disposable.

## How it works

```text
Test Definition
      │
      ▼
Create Pods 
      │
      ├── Create service containers
      ├── Create Podman pod
      ├── Initialize databases
      ├── Start services
      │
      ▼
 Execute API tests
      │
      ├── Requests
      ├── Contextual variables
      └── Assertions
      │
      ▼
 Test Results
      │
      ▼
 Tear down environment
```


## Key concept 

#### Database layers

Database initialization can be expressed as layers:

```xml
<Layer name="base-schema">
  <Sql source="./schema.sql" />
</Layer>

<Layer name="fahrenheit10" from="base-schema">
  <Sql source="./fahrenheit10.sql" />
</Layer>
```

This allows test scenarios to build on different database states.

#### Isolated environments

Spinner uses Podman pods to create isolated environments for each test
execution.

Services such as an API and PostgreSQL database can therefore be started
together with their required configuration.

## Example 

#### Description file :

Service description
``` xml
<Services>
    <Service name="db" image="postgres:17">
      <Key name="POSTGRES_USER" value="spiner" />
      <GeneratedKey name="POSTGRES_PASSWORD" len="32" seed="10" />
      <Key name="POSTGRES_DB" value="weddy" />
      <Key name="DB_CONNECTION_STRING" value="Server={{CONTAINER_NAME}};Port=5432;Database={{POSTGRES_DB}};User ID={{POSTGRES_USER}};Password={{POSTGRES_PASSWORD}};"/>

      <Layer name="base-schema" >
        <Sql source="./schema.sql"/>
      </Layer>

      <Layer name="fahrenheit10" from="base-schema" >
        <Sql source="./fahrenheit10.sql"/>
      </Layer>

      <Layer name="celsius10" from="base-schema">
        <Sql source="./celsius10.sql"/>
      </Layer>

      <Layer name="bothfandc" from="fahrenheit10,celsius10" >
      </Layer>
    </Service  >

    <Service name="api" build="/home/etienne/Desktop/repository/demo/tya/API.Dockerfile">
      <Key name="DB_CONNECTION_STRING"/>
    </Service>
  </Services>
</Spinner>
```

Request description
``` xml
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

    <Request name="patch" method="PATCH" path="weather">
      <Key name="id"/>
      <Key name="temperature"/>
      <Key name="type"/>

      <Body type="json">
        <Key name="id" value="{{id}}"/>
        <Key name="temperature" value="{{temperature}}"/>
        <Key name="type" value="{{type}}"/>
      </Body>
    </Request>
  </Requests>
``` 

Tests description
``` xml 

<!--Define each test scenario -->
<TestSuite description="Simple tests">
    <Stack>
      <Service name="db" layer="fahrenheit10"/>

      <Service name="api" target="true" logEnabled="true">
        <Arg from="db" key="DB_CONNECTION_STRING"/>
      </Service>
    </Stack>

    <Tests mode="sync" description="First set">
      <Key name="id" value=""/>
      <Key name="temperature" value="105"/>
      <Key name="type" value="Celsius"/>

      <Test description="step-1">
        <Request name="getall"/>
        <Asserts>
          <Equals actual="{{response['json']#type}}" expected="array"/>
          <Equals actual="{{response['json']#length}}" expected="10"/>
        </Asserts>
      </Test>
      <Test description="step-2">
        <Request name="add">
          <Arg key="temperature" value="{{temperature}}"/>
          <Arg key="type" value="{{type}}"/>
        </Request>

        <Response>
          <Set key="id" value="{{response['json']['id']}}"/>
        </Response>

        <Asserts>
          <NotNull key="id"/>
          <Equals actual="{{response['json']#type}}" expected="object"/>
          <Equals actual="{{response['json']['temperatureC']}}" expected="{{temperature}}"/>
        </Asserts>
      </Test>
     </Tests>
  </TestSuite>
``` 

## Status 

Spinner is an experimental / personal project exploring declarative API
integration testing and containerized test environments.

The project is under active development and the DSL/API may change.

## Technology
- C#
- .NET
- Podman
