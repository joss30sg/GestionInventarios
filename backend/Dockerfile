FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY InventoryManagementAPI.sln .
COPY InventoryManagementAPI.Api/InventoryManagementAPI.Api.csproj InventoryManagementAPI.Api/
COPY InventoryManagementAPI.Application/InventoryManagementAPI.Application.csproj InventoryManagementAPI.Application/
COPY InventoryManagementAPI.Domain/InventoryManagementAPI.Domain.csproj InventoryManagementAPI.Domain/
COPY InventoryManagementAPI.Infrastructure/InventoryManagementAPI.Infrastructure.csproj InventoryManagementAPI.Infrastructure/
RUN dotnet restore

COPY . .
RUN dotnet publish InventoryManagementAPI.Api/InventoryManagementAPI.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "InventoryManagementAPI.Api.dll"]
