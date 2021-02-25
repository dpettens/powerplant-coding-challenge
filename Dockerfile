# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY src/PowerPlantChallenge.WebApi/*.csproj ./src/PowerPlantChallenge.WebApi/
COPY tests/PowerPlantChallenge.WebApi.Tests/*.csproj ./tests/PowerPlantChallenge.WebApi.Tests/
RUN dotnet restore

# copy everything else and build full solution
COPY . .
RUN dotnet build

# run unit tests
WORKDIR /app/tests/PowerPlantChallenge.WebApi.Tests
RUN dotnet test --logger:trx

# publish the web api
FROM build AS publish
WORKDIR /app/src/PowerPlantChallenge.WebApi/
RUN dotnet publish -c Release -o out --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=publish /app/src/PowerPlantChallenge.WebApi/out ./
EXPOSE 80
ENTRYPOINT ["dotnet", "PowerPlantChallenge.WebApi.dll"]