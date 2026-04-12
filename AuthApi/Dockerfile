# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file FIRST (safe + fast restore cache)
COPY AuthApi.csproj ./

# Restore explicitly
RUN dotnet restore ./AuthApi.csproj

# Copy everything else
COPY . .

# Publish
RUN dotnet publish ./AuthApi.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthApi.dll"]
