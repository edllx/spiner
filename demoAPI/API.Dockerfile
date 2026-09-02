FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR src

# Step 1: Copy ONLY the .csproj file first
# This should rerun only if csproj change 
COPY ./simpleapi/*.csproj ./simpleapi/
RUN dotnet restore ./simpleapi/simpleapi.csproj

# Step Copy the rest publish: 
COPY ./simpleapi/ ./simpleapi/
WORKDIR /src/simpleapi
RUN dotnet publish -c release -o /app --no-restore 

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "simpleapi.dll"]
