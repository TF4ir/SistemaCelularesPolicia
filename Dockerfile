# Fase base para ejecución web
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

# Fase build para compilar la web
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiamos el .csproj y restauramos dependencias
COPY ["SistemaCelularesPolicia/SistemaCelularesPolicia.csproj", "SistemaCelularesPolicia/"]
RUN dotnet restore "SistemaCelularesPolicia/SistemaCelularesPolicia.csproj"

# Copiamos todo el código y compilamos
COPY . .
WORKDIR "/src/SistemaCelularesPolicia"
RUN dotnet build "SistemaCelularesPolicia.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase publish para generar build optimizado
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SistemaCelularesPolicia.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final: imagen lista para hosting web
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SistemaCelularesPolicia.dll"]
