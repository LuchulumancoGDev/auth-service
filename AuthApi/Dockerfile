# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file (correct path from context)
COPY AuthApi.csproj ./

RUN dotnet restore AuthApi.csproj

# Copy everything
COPY . .

RUN dotnet publish AuthApi.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthApi.dll"]# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file (correct path from context)
COPY AuthApi.csproj ./

RUN dotnet restore AuthApi.csproj

# Copy everything
COPY . .

RUN dotnet publish AuthApi.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthApi.dll"]