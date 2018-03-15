FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

# FROM microsoft/aspnetcore-build:2.0 AS build
# FROM microsoft/aspnetcore-build:2.1.300-preview1 as build
FROM microsoft/aspnetcore-build:1.0-2.0 as build
WORKDIR /src
COPY *.sln ./
# COPY Slabs.SimpleDockerApi/Slabs.SimpleDockerApi.csproj Slabs.SimpleDockerApi/
COPY . .
RUN dotnet restore Slabs.SimpleDockerApi.sln -nowarn:msb3202,nu1503
WORKDIR /src/Slabs.SimpleDockerApi
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

#HEALTHCHECK --interval=5s --timeout=3s --retries=3 \
        #CMD curl -f http://localhost/health/ || exit 1

ENTRYPOINT ["dotnet", "Slabs.SimpleDockerApi.dll"]
