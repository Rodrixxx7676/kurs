<div align="center">
  <img src="Images/LogoSinFondo.svg" alt="KURS Logo" width="120" />

  # KURS — Rumbo al futuro

  Plataforma tecnológica empresarial construida con .NET 10, Blazor WebAssembly y Oracle Database.

  ![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)
  ![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?style=flat-square&logo=blazor)
  ![Oracle](https://img.shields.io/badge/Oracle-Database-F80000?style=flat-square&logo=oracle)
  ![JWT](https://img.shields.io/badge/Auth-JWT-000000?style=flat-square&logo=jsonwebtokens)
  ![EF Core](https://img.shields.io/badge/EF_Core-9.0-512BD4?style=flat-square)
  ![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

  [Reportar bug](https://github.com/Rodrixxx7676/kurs/issues) · [Solicitar feature](https://github.com/Rodrixxx7676/kurs/issues)
</div>

---

## Sobre el proyecto

**KURS** es una plataforma de servicios tecnológicos que permite a empresas y proveedores interactuar a través de un portal web moderno. Incluye autenticación JWT, gestión de usuarios, mensajes de contacto y un portal de proveedores.

El proyecto tiene **tres capas** completamente separadas:

```
KURS
├── Front/          → Sitio web HTML/CSS/JS (landing, login, registro, contacto)
├── KursApi/        → API REST en ASP.NET Core 10 + Oracle EF Core
└── KursFront/      → Panel de administración en Blazor WebAssembly
```

---

## Stack tecnológico

### Backend — `KursApi`
| Tecnología | Uso |
|------------|-----|
| ASP.NET Core 10 | Framework principal de la API REST |
| Entity Framework Core 9 | ORM para Oracle Database |
| Oracle Database | Base de datos relacional |
| JWT Bearer | Autenticación y autorización |
| BCrypt.Net | Hashing seguro de contraseñas |

### Frontend — `Front` (landing pública)
| Tecnología | Uso |
|------------|-----|
| HTML5 / CSS3 | Estructura y estilos |
| JavaScript (Vanilla) | Interactividad y llamadas a la API |
| Google Fonts (Orbitron + Exo 2) | Tipografía |
| Tabler Icons | Iconografía |

### Panel Admin — `KursFront`
| Tecnología | Uso |
|------------|-----|
| Blazor WebAssembly (.NET 10) | SPA del panel de administración |
| C# | Lógica del cliente |

---

## Funcionalidades

### Sitio público
- **Landing page** con secciones: hero, servicios, beneficios, estadísticas animadas, CTA y footer
- **Formulario de contacto** con envío a la API
- **Portal de proveedores** con registro y validación de RUC
- **Diseño responsive** (mobile-first) con menú hamburger
- **Efecto glassmorphism** con estrellas y orbes CSS animados

### Autenticación
- Registro de usuarios con validación de contraseña fuerte
- Login con JWT + almacenamiento seguro en localStorage
- Sistema de niveles de acceso (1: Visitante → 4: Administrador)
- Navbar dinámico: muestra avatar del usuario si está logueado

### Panel de administración (Nivel 4)
- Gestión de usuarios registrados
- Lectura y marcado de mensajes de contacto
- Análisis de mercado por sectores
- Sistema de niveles de acceso

### API REST
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/login` | Iniciar sesión | No |
| POST | `/api/auth/registro` | Crear cuenta | No |
| GET | `/api/clientes` | Listar usuarios | JWT (Nivel 4) |
| GET | `/api/contacto` | Listar mensajes | JWT (Nivel 4) |
| POST | `/api/contacto` | Enviar mensaje | No |
| PUT | `/api/contacto/{id}/leido` | Marcar como leído | JWT (Nivel 4) |

---

## Capturas de pantalla

> Próximamente — el proyecto está en fase de despliegue.

---

## Cómo correrlo localmente

### Requisitos previos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Oracle Database (o Oracle Cloud Free Tier)
- Git

### 1. Clonar el repositorio
```bash
git clone https://github.com/Rodrixxx7676/kurs.git
cd kurs
```

### 2. Configurar la API
```bash
cd KursApi
```

Editar `appsettings.json` con tu cadena de conexión Oracle y clave JWT:
```json
{
  "ConnectionStrings": {
    "OracleDb": "User Id=usuario;Password=clave;Data Source=host:1521/servicio;"
  },
  "Jwt": {
    "Key": "tu-clave-secreta-minimo-32-caracteres",
    "Issuer": "KursApi",
    "Audience": "KursApp"
  }
}
```

Ejecutar la API:
```bash
dotnet run
# API disponible en http://localhost:5212
```

### 3. Abrir el sitio web
Abre `Front/PagePrincipal.html` directamente en el navegador, o sirve la carpeta con:
```bash
npx serve Front/
```

### 4. Correr el panel Blazor (opcional)
```bash
cd KursFront
dotnet run
```

---

## Estructura del proyecto

```
kurs/
├── Front/                    # Sitio público HTML/CSS/JS
│   ├── PagePrincipal.html    # Landing page principal
│   ├── Login.html            # Inicio de sesión
│   ├── Registro.html         # Crear cuenta
│   ├── Contacto.html         # Formulario de contacto
│   ├── Proveedores.html      # Portal de proveedores
│   └── Cuenta.html           # Panel de usuario
│
├── KursApi/                  # API REST ASP.NET Core
│   ├── Controllers/          # AuthController, ClientesController, ContactoController
│   ├── Models/               # Cliente, MensajeContacto
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Data/                 # DbContext (Oracle EF Core)
│   ├── Services/             # Lógica de negocio
│   └── Program.cs            # Configuración JWT, CORS, EF Core
│
├── KursFront/                # Panel admin Blazor WebAssembly
│   ├── Pages/                # Componentes de página
│   ├── Components/           # Componentes reutilizables
│   └── Services/             # Servicios HTTP
│
└── Images/                   # Logos SVG
```

---

## Despliegue

| Capa | Proveedor recomendado |
|------|-----------------------|
| Frontend (HTML) | Netlify / Azure Static Web Apps / GitHub Pages |
| API (.NET) | Railway / Azure App Service |
| Base de datos | Oracle Cloud Free Tier |

---

## Autor

**Francisco Rodrigo Ponte Villarroel**

- GitHub: [@Rodrixxx7676](https://github.com/Rodrixxx7676)
- LinkedIn: [linkedin.com/in/francisco-ponte](https://linkedin.com/in/francisco-ponte)
- Email: rodripontevillarroel@gmail.com

---

## Licencia

Distribuido bajo la licencia MIT. Ver `LICENSE` para más información.
