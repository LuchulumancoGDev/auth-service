FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy ONLY csproj first (forces correct path resolution)
COPY AuthApi.csproj ./

# Debug step (VERY IMPORTANT)
RUN ls -la

# Restore
RUN dotnet restore ./AuthApi.csproj

# Copy everything AFTER restore
COPY . .

# Publish
RUN dotnet publish ./AuthApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthApi.dll"]
