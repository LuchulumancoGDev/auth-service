FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

# restore solution
RUN dotnet restore "./AuthApi.sln"

# publish solution
RUN dotnet publish "./AuthApi.sln" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthApi.dll"]
