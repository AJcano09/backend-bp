# Etapa base para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Tolerancia a timeouts de red con nuget.org: retry con backoff exponencial + timeout por request de 5 min
ENV NUGET_ENABLE_ENHANCED_HTTP_RETRY=true
ENV NUGET_HTTP_TIMEOUT=300
WORKDIR /src

# Copiar los archivos de proyecto (.csproj) para restaurar dependencias
COPY ["src/Services/ClientService/ClientService.Api/ClientService.Api.csproj", "src/Services/ClientService/ClientService.Api/"]
COPY ["src/Services/ClientService/ClientService.Infrastructure/ClientService.Infrastructure.csproj", "src/Services/ClientService/ClientService.Infrastructure/"]
COPY ["src/Services/ClientService/ClientService.Application/ClientService.Application.csproj", "src/Services/ClientService/ClientService.Application/"]
COPY ["src/Services/ClientService/ClientService.Domain/ClientService.Domain.csproj", "src/Services/ClientService/ClientService.Domain/"]
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked dotnet restore "src/Services/ClientService/ClientService.Api/ClientService.Api.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/src/Services/ClientService/ClientService.Api"
RUN dotnet build "ClientService.Api.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "ClientService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClientService.Api.dll"]