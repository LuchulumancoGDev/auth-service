FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

# Debug (you already confirmed this)
RUN ls -la
RUN ls -la AuthApi

# Restore
RUN dotnet restore AuthApi/AuthApi.csproj

# Build
RUN dotnet publish AuthApi/AuthApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthApi.dll"]
