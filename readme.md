# MicroJack.API

API para el sistema de control de acceso vehicular "MicroJack".

## Descripción

MicroJack.API es un servicio backend desarrollado en .NET 8 que gestiona el pre-registro y registro de visitantes, y controla una barrera física a través de un Phidget Interface Kit. La API expone endpoints para interactuar con una base de datos MongoDB y el hardware Phidget.

## Características

- **Gestión de Pre-Registros:** Permite a los residentes pre-registrar las placas de sus visitantes.
- **Gestión de Registros:** Permite a los guardias de seguridad registrar la entrada y salida de visitantes.
- **Control de Hardware:** Interactúa con un Phidget Interface Kit para controlar una barrera vehicular.
- **Documentación de API:** Expone una interfaz Swagger para la exploración y prueba de los endpoints.
- **Configuración Flexible:** Utiliza `appsettings.json` para una fácil configuración de la base de datos y CORS.

## Endpoints de la API

La API está organizada en tres grupos principales de endpoints:

### Pre-Registros (`/api/preregistrations`)

| Método | Ruta                               | Descripción                                                                 |
|--------|------------------------------------|-----------------------------------------------------------------------------|
| POST   | `/`                                | Crea un nuevo pre-registro.                                                 |
| GET    | `/by-plate/{plate}`                | Obtiene un pre-registro pendiente por número de placa.                        |
| GET    | `/`                                | Obtiene todos los pre-registros, con una opción de búsqueda.                |
| PATCH  | `/{id}/status`                     | Actualiza el estado de un pre-registro (ej. "pendiente", "completado").     |

### Registros (`/api/registrations`)

| Método | Ruta      | Descripción                                                                 |
|--------|-----------|-----------------------------------------------------------------------------|
| POST   | `/`       | Crea un nuevo registro de entrada/salida.                                   |
| GET    | `/`       | Obtiene todos los registros, con una opción de búsqueda.                    |
| GET    | `/{id}`   | Obtiene un registro específico por su ID.                                   |

### Phidget Test (`/api/phidget-test`)

| Método | Ruta                 | Descripción                                                                 |
|--------|----------------------|-----------------------------------------------------------------------------|
| POST   | `/initialize`        | Inicializa la conexión con el Phidget Interface Kit.                        |
| POST   | `/relay/{channel}/toggle` | Cambia el estado (ON/OFF) de un relé específico (0-3).                   |
| GET    | `/status`            | Obtiene el estado actual de todos los relés.                                |
| POST   | `/close`             | Cierra la conexión con el Phidget y apaga todos los relés.                  |

## Cómo Empezar

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MongoDB](https://www.mongodb.com/try/download/community)
- Opcional: Un Phidget Interface Kit 0/0/4 para probar la funcionalidad de hardware.

### Instalación

1. Clona el repositorio:
   ```bash
   git clone https://github.com/Proyectos-ITESO/api.git
   cd api
   ```

2. Configura `appsettings.Development.json` con tu cadena de conexión de MongoDB:
   ```json
   {
     "MongoDbSettings": {
       "ConnectionString": "mongodb://localhost:27017",
       "DatabaseName": "MicroJackDB"
     },
     "CorsSettings": {
       "AllowedOrigins": [
         "http://localhost:3000"
       ]
     }
   }
   ```

3. Ejecuta la aplicación:
   ```bash
   dotnet run
   ```

La API estará disponible en `https://localhost:7123` (o un puerto similar) y la interfaz de Swagger se encontrará en la raíz (`/`).

## Contribuciones

Las contribuciones son bienvenidas. Por favor, abre un "issue" para discutir cambios mayores o un "pull request" con tus mejoras.