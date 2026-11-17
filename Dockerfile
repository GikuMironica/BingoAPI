# Hopaut API – modular monolith host (src/Host/Hopaut.Api.Host).
# Build from the repository root. ImageSharp needs the Six Labors Community license for Release builds (blocker B-002 in TODO.md);
# pass the git-ignored sixlabors.lic as a build secret, never as a build argument:
#   podman build --secret id=sixlabors_lic,src=sixlabors.lic -t hopaut-api .     (docker build accepts the same flags)
# Podman's Windows client cannot stage build secrets ("creating temp file … podman-build-secret"); build inside the machine instead:
#   podman machine ssh "cd /mnt/c/<path-to-repo> && sudo podman build --secret id=sixlabors_lic,src=sixlabors.lic -t hopaut-api ."
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ src/
RUN --mount=type=secret,id=sixlabors_lic,target=/src/sixlabors.lic \
    dotnet publish src/Host/Hopaut.Api.Host/Hopaut.Api.Host.csproj -c Release -o /app/publish -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Non-root user predefined by the .NET images; the app listens on ASPNETCORE_HTTP_PORTS (8080 by default).
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Hopaut.Api.Host.dll"]
