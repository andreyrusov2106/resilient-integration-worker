FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IntegrationWorker.Service/IntegrationWorker.Service.csproj", "IntegrationWorker.Service/"]
RUN dotnet restore "IntegrationWorker.Service/IntegrationWorker.Service.csproj"
COPY . .
WORKDIR "/src/IntegrationWorker.Service"
RUN dotnet build "IntegrationWorker.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IntegrationWorker.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IntegrationWorker.Service.dll"]