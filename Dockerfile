# ── Etapa de build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaurar dependencias primero (aprovecha la caché de Docker)
COPY KursApi/KursApi.csproj KursApi/
COPY KursFront/KursFront.csproj KursFront/
RUN dotnet restore KursApi/KursApi.csproj && dotnet restore KursFront/KursFront.csproj

COPY KursApi/ KursApi/
COPY KursFront/ KursFront/

RUN dotnet publish KursFront/KursFront.csproj -c Release -o /out/front
RUN dotnet publish KursApi/KursApi.csproj    -c Release -o /out/api

# El Blazor WASM publicado se sirve como archivos estáticos desde la API
RUN mkdir -p /out/api/wwwroot && cp -r /out/front/wwwroot/. /out/api/wwwroot/

# ── Etapa de runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out/api .

# Render/Railway inyectan PORT; localmente usa 8080
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet KursApi.dll"]
