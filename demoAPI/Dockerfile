FROM mcr.microsoft.com/dotnet/sdk:9.0 AS sdk

FROM sdk AS build
WORKDIR src
COPY ./simpleapi/ ./simpleapi/

WORKDIR simpleapi
RUN dotnet publish -c release -o app simpleapi.csproj 

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /
COPY --from=build ./src/simpleapi/app .
ENTRYPOINT ["dotnet", "simpleapi.dll"]
