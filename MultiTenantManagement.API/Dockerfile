FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore MultiTenantManagement.API/MultiTenantManagement.API/MultiTenantManagement.API.csproj

RUN dotnet publish MultiTenantManagement.API/MultiTenantManagement.API/MultiTenantManagement.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MultiTenantManagement.API.dll"]
