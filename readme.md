# MicroJack API

API de gestión de registros y pre-registros para control de acceso vehicular.

## 📋 Descripción

MicroJack API es un servicio backend diseñado para gestionar el control de acceso vehicular a través de registros y pre-registros. Permite registrar entradas de vehículos, gestionar pre-registros para visitantes esperados y mantener un historial completo de accesos.

## 🚀 Características

- **Gestión de Registros**: Crear y consultar registros de entrada de vehículos
- **Sistema de Pre-registros**: Permite pre-registrar visitantes con sus datos vehiculares
- **Búsqueda por placas**: Búsqueda eficiente de registros y pre-registros por número de placa
- **API RESTful**: Endpoints claros y documentados con Swagger
- **Base de datos MongoDB**: Almacenamiento persistente y escalable
- **Arquitectura modular**: Código organizado por dominios y responsabilidades

## 🛠️ Tecnologías

- .NET 6.0
- MongoDB
- Swagger/OpenAPI
- Docker (opcional)

## 📦 Estructura del Proyecto

```
src/
├── Models/
│   ├── Registration.cs           # Modelo para registros
│   ├── PreRegistration.cs        # Modelo para pre-registros
│   └── MongoDbSettings.cs        # Configuración de MongoDB
├── Services/
│   ├── Interfaces/               # Interfaces de servicios
│   │   ├── IMongoService.cs
│   │   ├── IRegistrationService.cs
│   │   └── IPreRegistrationService.cs
│   ├── BaseMongoService.cs       # Servicio base de MongoDB
│   ├── RegistrationService.cs    # Servicio de registros
│   └── PreRegistrationService.cs # Servicio de pre-registros
├── Routes/
│   ├── ApiRoutes.cs              # Orquestador de rutas
│   └── Modules/
│       ├── RegistrationRoutes.cs # Rutas de registros
│       └── PreRegistrationRoutes.cs # Rutas de pre-registros
└── Program.cs                    # Punto de entrada
```

## 🚦 Requisitos Previos

- .NET 6.0 SDK
- MongoDB (local o remoto)
- Git

## ⚙️ Configuración

1. Clona el repositorio:
```bash
git clone https://github.com/tu-usuario/microjack-api.git
cd microjack-api
```

2. Configura las variables de entorno en `appsettings.json`:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "tu_connection_string",
    "DatabaseName": "nombre_de_tu_base_de_datos"
  },
  "CorsSettings": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

3. Restaura las dependencias:
```bash
dotnet restore
```

## 🏃‍♂️ Ejecución

1. Ejecuta la aplicación:
```bash
dotnet run
```

2. Accede a Swagger UI: `http://localhost:5000/` (en desarrollo)

## 📚 API Endpoints

### Registros

- `GET /api/registrations` - Obtiene todos los registros
- `GET /api/registrations/{id}` - Obtiene un registro por ID
- `POST /api/registrations` - Crea un nuevo registro
- `GET /api/registrations?search={placa}` - Busca registros por placa

### Pre-registros

- `GET /api/preregistrations` - Obtiene todos los pre-registros
- `POST /api/preregistrations` - Crea un nuevo pre-registro
- `GET /api/preregistrations/by-plate/{plate}` - Busca pre-registro pendiente por placa
- `PATCH /api/preregistrations/{id}/status` - Actualiza el estado de un pre-registro

## 📝 Modelos de Datos

### Registration
```json
{
  "id": "string",
  "registrationType": "string",
  "house": "string",
  "visitReason": "string",
  "visitorName": "string",
  "visitedPerson": "string",
  "guard": "string",
  "comments": "string",
  "folio": "string",
  "entryTimestamp": "datetime",
  "plates": "string",
  "brand": "string",
  "color": "string",
  "status": "string",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### PreRegistration
```json
{
  "id": "string",
  "plates": "string",
  "visitorName": "string",
  "brand": "string",
  "color": "string",
  "houseVisited": "string",
  "arrivalDateTime": "datetime",
  "personVisited": "string",
  "status": "string",
  "createdBy": "string",
  "createdAt": "datetime"
}
```

## 🔒 Estados

- **PENDIENTE**: Pre-registro creado, esperando llegada
- **INGRESADO**: Visitante ha ingresado
- **CERRADO**: Visita finalizada
- **CANCELADO**: Pre-registro cancelado

## 🧪 Pruebas

Para ejecutar las pruebas unitarias:
```bash
dotnet test
```

