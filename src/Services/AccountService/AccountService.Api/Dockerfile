# Etapa base para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar los archivos de proyecto (.csproj) para restaurar dependencias
COPY ["src/Services/AccountService/AccountService.Api/AccountService.Api.csproj", "src/Services/AccountService/AccountService.Api/"]
COPY ["src/Services/AccountService/AccountService.Infrastructure/AccountService.Infrastructure.csproj", "src/Services/AccountService/AccountService.Infrastructure/"]
COPY ["src/Services/AccountService/AccountService.Application/AccountService.Application.csproj", "src/Services/AccountService/AccountService.Application/"]
COPY ["src/Services/AccountService/AccountService.Domain/AccountService.Domain.csproj", "src/Services/AccountService/AccountService.Domain/"]
RUN dotnet restore "src/Services/AccountService/AccountService.Api/AccountService.Api.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/src/Services/AccountService/AccountService.Api"
RUN dotnet build "AccountService.Api.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "AccountService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountService.Api.dll"]