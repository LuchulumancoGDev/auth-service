FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy EVERYTHING into container
COPY AuthApi/ ./AuthApi/

WORKDIR /src/AuthApi

# NOW the csproj definitely exists here
RUN ls -la

RUN dotnet restore AuthApi.csproj
RUN dotnet publish AuthApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthApi.dll"]
