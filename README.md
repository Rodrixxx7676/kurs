# KURS · versión .NET

Primera plataforma de **KURS**, un estudio de desarrollo web y automatización
para pequeñas y medianas empresas. Construida sobre .NET 10 con una API REST y
un frontend en Blazor WebAssembly.

> La web que hoy está en producción es la versión en Node.js:
> [kurs-seenode](https://github.com/Rodrixxx7676/kurs-seenode) ·
> [kurs-seenode.seenode.app](https://kurs-seenode.seenode.app)
>
> Este repositorio conserva la arquitectura original en .NET, con la que se
> resolvieron la autenticación, el modelo de datos y el acceso a PostgreSQL.

## Arquitectura

```
KursApi/      API REST en ASP.NET Core
  Controllers/  Puntos de entrada HTTP
  Services/     Reglas de negocio
  Data/         Contexto de Entity Framework Core
  Models/       Entidades del dominio
  DTOs/         Formas de los datos que viajan por la API
KursFront/    Cliente en Blazor WebAssembly
  Pages/        Portada, acceso y contacto
Front/        Maquetas HTML previas a la migración a Blazor
Dockerfile    Imagen para desplegar la API
```

La separación entre `Controllers`, `Services` y `Data` mantiene las reglas de
negocio fuera tanto del transporte HTTP como del acceso a datos: se puede
cambiar la base de datos sin tocar la lógica, y al revés.

## Tecnologías

| Capa | Tecnología |
|---|---|
| API | ASP.NET Core sobre .NET 10 |
| Frontend | Blazor WebAssembly |
| Datos | PostgreSQL con Entity Framework Core (Npgsql) |
| Autenticación | JWT Bearer y contraseñas cifradas con BCrypt |
| Despliegue | Docker |

## Cómo levantarlo

Hace falta el SDK de .NET 10 y una base PostgreSQL en marcha.

```bash
dotnet restore
dotnet run --project KursApi
dotnet run --project KursFront   # en otra terminal
```

La cadena de conexión y la clave de firma se leen de la configuración. Para
desarrollo local conviene mantenerlas fuera del repositorio:

```bash
dotnet user-secrets init --project KursApi
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=kurs;Username=usuario;Password=clave" --project KursApi
dotnet user-secrets set "Jwt:Key" "una-clave-larga-y-privada" --project KursApi
```

## Con Docker

```bash
docker build -t kurs-api .
docker run -p 8080:8080 kurs-api
```
