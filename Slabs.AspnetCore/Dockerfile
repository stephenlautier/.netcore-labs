FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 5001
EXPOSE 5443

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Slabs.AspnetCore/Slabs.AspnetCore.csproj Slabs.AspnetCore/
RUN dotnet restore Slabs.AspnetCore/Slabs.AspnetCore.csproj
COPY . .
WORKDIR /src/Slabs.AspnetCore
RUN dotnet build Slabs.AspnetCore.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Slabs.AspnetCore.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Slabs.AspnetCore.dll"]
