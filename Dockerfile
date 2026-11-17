# Hopaut API – modular monolith host (src/Host/Hopaut.Api.Host).
# Build from the repository root:  docker build -t hopaut-api .   (or: podman build -t hopaut-api .)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ src/
RUN dotnet publish src/Host/Hopaut.Api.Host/Hopaut.Api.Host.csproj -c Release -o /app/publish -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Non-root user predefined by the .NET images; the app listens on ASPNETCORE_HTTP_PORTS (8080 by default).
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Hopaut.Api.Host.dll"]
