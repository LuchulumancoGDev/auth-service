FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy everything from AuthApi folder
COPY . .

# sanity check (optional but useful)
RUN ls -la

# ✔ CORRECT PATH (root of context)
RUN dotnet restore AuthApi.csproj
RUN dotnet publish AuthApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthApi.dll"]
