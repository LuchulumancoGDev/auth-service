FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything inside AuthApi context
COPY . .

# Debug (important for CI visibility)
RUN ls -la

# Restore using explicit project path
RUN dotnet restore AuthApi.csproj

RUN dotnet publish AuthApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthApi.dll"]
