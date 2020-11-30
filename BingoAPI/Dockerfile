FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build

ARG BUILDCONFIG=RELEASE
ARG VERSION=1.0.0

COPY BingoAPI.csproj /build/
RUN dotnet restore ./build/BingoAPI.csproj

COPY . ./build/
WORKDIR /build/
RUN dotnet publish ./BingoAPI.csproj -c $BUILDCONFIG -o out /p:Version=$VERSION

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /build/out .
ENTRYPOINT ["dotnet", "BingoAPI.dll"] 