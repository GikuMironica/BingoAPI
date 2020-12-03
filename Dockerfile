# FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build

# ARG BUILDCONFIG=RELEASE
# ARG VERSION=1.0.0

# COPY ["BingoAPI/BingoAPI.csproj", "/build/"]
# COPY ["Bingo.Contracts/Bingo.Contracts.csproj", "BingoAPI.Contracts/"]
# RUN dotnet restore ./build/BingoAPI.csproj

# COPY . ./build/
# WORKDIR /build/
# RUN dotnet publish ./BingoAPI.csproj -c $BUILDCONFIG -o out /p:Version=$VERSION

# FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
# WORKDIR /app
# COPY --from=build /build/out .
# ENTRYPOINT ["dotnet", "BingoAPI.dll"]

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /src
COPY ["BingoAPI/BingoAPI.csproj", "BingoAPI/"]
COPY ["Bingo.Contracts/Bingo.Contracts.csproj", "BingoAPI.Contracts/"]
RUN dotnet restore BingoAPI/BingoAPI.csproj
COPY . .
WORKDIR "/src/BingoAPI"
RUN dotnet build "BingoAPI.csproj" -c Release -o /app

FROM build as publish
RUN dotnet publish "BingoAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT [ "dotnet", "BingoAPI.dll" ]
