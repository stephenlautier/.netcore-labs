FROM microsoft/aspnetcore:1.1
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-obj/Docker/publish} .

HEALTHCHECK --interval=5s --timeout=3s --retries=3 \
        CMD curl -f http://localhost/health/ || exit 1

ENTRYPOINT ["dotnet", "Slabs.SimpleDockerApi.dll"]
