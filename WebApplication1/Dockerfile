FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json ./
COPY WebApplication1/WebApplication1.csproj WebApplication1/
COPY WebApplication1/packages.lock.json WebApplication1/
RUN dotnet restore WebApplication1/WebApplication1.csproj --locked-mode
COPY WebApplication1/ WebApplication1/
RUN dotnet publish WebApplication1/WebApplication1.csproj -c Release -o /app/publish --no-restore
RUN dotnet tool install dotnet-ef --tool-path /tools --version 10.0.12

FROM build AS migrate
ENTRYPOINT ["/tools/dotnet-ef", "database", "update", "--project", "WebApplication1/WebApplication1.csproj", "--startup-project", "WebApplication1/WebApplication1.csproj", "--configuration", "Release", "--no-build"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/data/uploads /app/data/keys && chown -R app:app /app/data
USER app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "WebApplication1.dll"]
