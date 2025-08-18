# 📋 Guía Completa de Endpoints del API - RKS Microjack

Esta guía documenta todos los endpoints disponibles en el API de MicroJack PRO, incluyendo ejemplos reales de respuestas obtenidas del sistema.

## 🌐 Configuración Base

- **URL Base:** `http://localhost:5134/api`
- **Autenticación:** JWT Bearer Token
- **Content-Type:** `application/json`

## 🔐 Autenticación

### Login
```bash
curl -X POST "http://localhost:5134/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "guard": {
    "id": 1,
    "username": "admin",
    "fullName": "Super Administrador",
    "isActive": true,
    "roles": ["SuperAdmin"],
    "isAdmin": true
  }
}
```

### Verificar Estado del Servicio
```bash
curl -X GET "http://localhost:5134/api/auth/health"
```

**Respuesta:**
```json
{
  "success": true,
  "message": "Authentication service is healthy",
  "timestamp": "2025-08-18T13:54:25.9197803Z",
  "policies": [
    "GuardLevel: Guard, Admin, SuperAdmin",
    "AdminLevel: Admin, SuperAdmin", 
    "SuperAdminLevel: SuperAdmin"
  ]
}
```

### Información del Usuario Actual
```bash
curl -X GET "http://localhost:5134/api/auth/me" \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta:**
```json
{
  "success": true,
  "guard": {
    "id": 1,
    "username": "admin",
    "fullName": "Super Administrador",
    "roles": ["SuperAdmin"],
    "isAdmin": true
  }
}
```

## 🚗 Catálogos de Vehículos

### Marcas de Vehículos
```bash
curl -X GET "http://localhost:5134/api/catalogs/vehicle-brands" \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta (35 marcas disponibles):**
```json
{
  "success": true,
  "data": [
    {"id": 1, "name": "Nissan"},
    {"id": 2, "name": "Toyota"},
    {"id": 3, "name": "Chevrolet"},
    {"id": 4, "name": "Ford"},
    {"id": 5, "name": "Honda"},
    {"id": 6, "name": "Mazda"},
    {"id": 7, "name": "Hyundai"},
    {"id": 8, "name": "Kia"},
    {"id": 9, "name": "Volkswagen"},
    {"id": 10, "name": "BMW"},
    {"id": 11, "name": "Mercedes-Benz"},
    {"id": 12, "name": "Audi"},
    {"id": 13, "name": "Peugeot"},
    {"id": 14, "name": "Renault"},
    {"id": 15, "name": "Jeep"},
    {"id": 16, "name": "Dodge"},
    {"id": 17, "name": "Chrysler"},
    {"id": 18, "name": "Mitsubishi"},
    {"id": 19, "name": "Subaru"},
    {"id": 20, "name": "Suzuki"},
    {"id": 21, "name": "Isuzu"},
    {"id": 22, "name": "JAC"},
    {"id": 23, "name": "SEAT"},
    {"id": 24, "name": "Fiat"},
    {"id": 25, "name": "Alfa Romeo"},
    {"id": 26, "name": "Volvo"},
    {"id": 27, "name": "Infiniti"},
    {"id": 28, "name": "Lexus"},
    {"id": 29, "name": "Acura"},
    {"id": 30, "name": "Cadillac"},
    {"id": 31, "name": "Lincoln"},
    {"id": 32, "name": "Buick"},
    {"id": 33, "name": "GMC"},
    {"id": 34, "name": "Pontiac"},
    {"id": 35, "name": "Otro"}
  ]
}
```

### Colores de Vehículos
```bash
curl -X GET "http://localhost:5134/api/catalogs/vehicle-colors" \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta (22 colores disponibles):**
```json
{
  "success": true,
  "data": [
    {"id": 1, "name": "Blanco"},
    {"id": 2, "name": "Negro"},
    {"id": 3, "name": "Gris"},
    {"id": 4, "name": "Plata"},
    {"id": 5, "name": "Azul"},
    {"id": 6, "name": "Rojo"},
    {"id": 7, "name": "Verde"},
    {"id": 8, "name": "Amarillo"},
    {"id": 9, "name": "Naranja"},
    {"id": 10, "name": "Café"},
    {"id": 11, "name": "Beige"},
    {"id": 12, "name": "Dorado"},
    {"id": 13, "name": "Morado"},
    {"id": 14, "name": "Rosa"},
    {"id": 15, "name": "Turquesa"},
    {"id": 16, "name": "Azul Marino"},
    {"id": 17, "name": "Gris Oscuro"},
    {"id": 18, "name": "Gris Claro"},
    {"id": 19, "name": "Rojo Vino"},
    {"id": 20, "name": "Verde Militar"},
    {"id": 21, "name": "Azul Rey"},
    {"id": 22, "name": "Otro"}
  ]
}
```

### Tipos de Vehículos
```bash
curl -X GET "http://localhost:5134/api/catalogs/vehicle-types" \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta (14 tipos disponibles):**
```json
{
  "success": true,
  "data": [
    {"id": 1, "name": "Automóvil"},
    {"id": 2, "name": "Camioneta"},
    {"id": 3, "name": "SUV"},
    {"id": 4, "name": "Motocicleta"},
    {"id": 5, "name": "Bicicleta"},
    {"id": 6, "name": "Camión"},
    {"id": 7, "name": "Van"},
    {"id": 8, "name": "Pick-up"},
    {"id": 9, "name": "Deportivo"},
    {"id": 10, "name": "Convertible"},
    {"id": 11, "name": "Hatchback"},
    {"id": 12, "name": "Sedán"},
    {"id": 13, "name": "Coupé"},
    {"id": 14, "name": "Peatón"}
  ]
}
```

## 🏠 Gestión de Casas

### Obtener Todas las Casas
```bash
curl -X GET "http://localhost:5134/api/casas" \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta (10 casas registradas):**
```json
{
  "success": true,
  "count": 10,
  "data": [
    {
      "id": 1,
      "casa": "102",
      "extension": "102-1",
      "representativeName": "Juan Pérez García",
      "representativePhone": null,
      "representativeResidentId": 1,
      "residentCount": 2
    },
    {
      "id": 4,
      "casa": "15",
      "extension": "15-B",
      "representativeName": "Ana Martínez Torres",
      "representativePhone": null,
      "representativeResidentId": 4,
      "residentCount": 1
    },
    {
      "id": 7,
      "casa": "156",
      "extension": "156-1",
      "representativeName": "Roberto Silva Morales",
      "representativePhone": null,
      "representativeResidentId": 7,
      "residentCount": 1
    },
    {
      "id": 3,
      "casa": "207",
      "extension": "207-2",
      "representativeName": "Carlos Ramírez Sánchez",
      "representativePhone": null,
      "representativeResidentId": 3,
      "residentCount": 1
    },
    {
      "id": 9,
      "casa": "234",
      "extension": "234-2",
      "representativeName": "Fernando Castro Vega",
      "representativePhone": null,
      "representativeResidentId": 9,
      "residentCount": 1
    },
    {
      "id": 5,
      "casa": "301",
      "extension": "301-3",
      "representativeName": "Pedro González Villa",
      "representativePhone": null,
      "representativeResidentId": 5,
      "residentCount": 1
    },
    {
      "id": 6,
      "casa": "42",
      "extension": "42-C",
      "representativeName": "Laura Jiménez Cruz",
      "representativePhone": null,
      "representativeResidentId": 6,
      "residentCount": 1
    },
    {
      "id": 2,
      "casa": "5",
      "extension": "5-A",
      "representativeName": "María López Hernández",
      "representativePhone": null,
      "representativeResidentId": 2,
      "residentCount": 1
    },
    {
      "id": 10,
      "casa": "67",
      "extension": "67-A",
      "representativeName": "Diana Moreno Aguilar",
      "representativePhone": null,
      "representativeResidentId": 10,
      "residentCount": 1
    },
    {
      "id": 8,
      "casa": "89",
      "extension": "89-D",
      "representativeName": "Carmen Ruiz Flores",
      "representativePhone": null,
      "representativeResidentId": 8,
      "residentCount": 1
    }
  ]
}
```

## 📝 Gestión de Bitácora

### Obtener Notas de Bitácora
```bash
curl -X GET "http://localhost:5134/api/bitacora" \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta (5 notas disponibles):**
```json
{
  "success": true,
  "count": 5,
  "filters": {
    "guardId": null,
    "fechaInicio": null,
    "fechaFin": null
  },
  "data": [
    {
      "id": 5,
      "note": "CAcade perero",
      "timestamp": "2025-07-27T10:36:05.5993435",
      "guardId": 1,
      "guardName": "admin"
    },
    {
      "id": 4,
      "note": "el patron nos tumbo el jale jaja\ndefef",
      "timestamp": "2025-07-27T10:35:09.4478779",
      "guardId": 1,
      "guardName": "admin"
    },
    {
      "id": 3,
      "note": "dassdadw",
      "timestamp": "2025-07-27T10:27:27.1748656",
      "guardId": 1,
      "guardName": "admin"
    },
    {
      "id": 2,
      "note": "fefefefe",
      "timestamp": "2025-07-27T09:52:28.3312877",
      "guardId": 1,
      "guardName": "admin"
    },
    {
      "id": 1,
      "note": "adada",
      "timestamp": "2025-07-27T09:48:02.8812972",
      "guardId": 1,
      "guardName": "admin"
    }
  ]
}
```

### Crear Nota de Bitácora
```bash
curl -X POST "http://localhost:5134/api/bitacora" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"note": "Nueva nota de prueba"}'
```

## 📅 Pre-registros

### Obtener Pre-registros Pendientes
```bash
curl -X GET "http://localhost:5134/api/preregistro/pendientes" \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta (3 pre-registros pendientes):**
```json
{
  "success": true,
  "count": 3,
  "data": [
    {
      "id": 3,
      "plates": "DEF-456",
      "visitorName": "Carlos Eduardo Mendoza",
      "vehicleBrand": "Nissan",
      "vehicleColor": "Negro",
      "houseVisited": "207",
      "expectedArrivalTime": "2025-01-27T20:00:00",
      "personVisited": "Carlos Ramírez Sánchez",
      "status": "PENDIENTE",
      "comments": "Reunión de trabajo",
      "createdAt": "2025-07-27T09:20:36.6767447",
      "expiresAt": null,
      "createdBy": "admin"
    },
    {
      "id": 2,
      "plates": "XYZ-789",
      "visitorName": "Ana Sofía Rodríguez",
      "vehicleBrand": "Honda",
      "vehicleColor": "Azul",
      "houseVisited": "5",
      "expectedArrivalTime": "2025-01-27T18:30:00",
      "personVisited": "María López Hernández",
      "status": "PENDIENTE",
      "comments": "Entrega de paquete importante",
      "createdAt": "2025-07-27T09:20:21.3217788",
      "expiresAt": null,
      "createdBy": "admin"
    },
    {
      "id": 1,
      "plates": "ABC-123",
      "visitorName": "José Martinez García",
      "vehicleBrand": "Toyota",
      "vehicleColor": "Blanco",
      "houseVisited": "102",
      "expectedArrivalTime": "2025-01-27T16:00:00",
      "personVisited": "Juan Pérez García",
      "status": "PENDIENTE",
      "comments": "Visita médica programada",
      "createdAt": "2025-07-27T09:20:00.4950991",
      "expiresAt": null,
      "createdBy": "admin"
    }
  ]
}
```

## 🔄 Control de Acceso

### Obtener Visitas Activas
```bash
curl -X GET "http://localhost:5134/api/access/active-visits" \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta (actualmente sin visitas activas):**
```json
{
  "success": true,
  "count": 0,
  "data": []
}
```

### Obtener Logs de Acceso
```bash
curl -X GET "http://localhost:5134/api/accesslogs" \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta (7 registros de acceso, todos con status "FUERA"):**
```json
{
  "success": true,
  "data": [
    {
      "id": 7,
      "entryTimestamp": "2025-07-27T11:52:56",
      "exitTimestamp": "2025-07-27T05:53:06",
      "status": "FUERA",
      "comments": "Registro de prueba para testing. Exit: ",
      "visitorId": 6,
      "vehicleId": 1,
      "addressId": 1,
      "residentVisitedId": null,
      "entryGuardId": 1,
      "exitGuardId": 1,
      "visitReasonId": null,
      "gafeteNumber": null,
      "createdAt": "2025-07-27T05:52:56.481883",
      "updatedAt": "2025-07-27T11:53:06.3332783"
    },
    {
      "id": 5,
      "entryTimestamp": "2025-07-27T11:06:03.426",
      "exitTimestamp": "2025-07-27T11:46:34.3653978",
      "status": "FUERA",
      "comments": null,
      "visitorId": 4,
      "vehicleId": 13,
      "addressId": 8,
      "residentVisitedId": null,
      "entryGuardId": 1,
      "exitGuardId": 1,
      "visitReasonId": null,
      "gafeteNumber": null,
      "createdAt": "2025-07-27T11:06:03.5167079",
      "updatedAt": "2025-07-27T11:46:34.3655797"
    }
    // ... más registros con estructura similar
  ]
}
```

## 📊 Estructura de Datos Común

### Patrón de Respuesta Exitosa
Todos los endpoints siguen este patrón:
```json
{
  "success": true,
  "data": [...],      // Array de objetos o objeto único
  "count": X,         // Para endpoints que retornan listas (opcional)
  "message": "...",   // Mensaje descriptivo (opcional)
  "filters": {...}    // Filtros aplicados (opcional)
}
```

### Patrón de Respuesta de Error
```json
{
  "success": false,
  "message": "Descripción del error",
  "error": "Detalles técnicos del error"
}
```

## 🔧 Endpoints Adicionales Disponibles

Según el código fuente (`ApiService.js`), también están disponibles los siguientes endpoints:

### Gestión de Guardias
- `GET /guards` - Obtener lista de guardias
- `GET /guards/{id}` - Obtener guardia específico
- `POST /guards` - Crear nuevo guardia
- `PUT /guards/{id}` - Actualizar guardia
- `DELETE /guards/{id}` - Eliminar guardia

### Gestión de Visitantes
- `GET /visitors` - Obtener visitantes
- `GET /visitors/{id}` - Obtener visitante específico
- `POST /visitors` - Crear visitante
- `PUT /visitors/{id}` - Actualizar visitante
- `DELETE /visitors/{id}` - Eliminar visitante

### Gestión de Vehículos
- `GET /vehicles` - Obtener vehículos
- `GET /vehicles/{id}` - Obtener vehículo específico
- `GET /vehicles/plate/{licensePlate}` - Buscar por placa
- `POST /vehicles` - Crear vehículo
- `PUT /vehicles/{id}` - Actualizar vehículo
- `DELETE /vehicles/{id}` - Eliminar vehículo

### Subida de Archivos
- `POST /upload/image` - Subir imagen individual
- `POST /upload/images` - Subir múltiples imágenes
- `DELETE /upload/image` - Eliminar imagen

### Control de Acceso Principal
- `POST /access/register-entry` - Registrar entrada
- `POST /access/register-exit/{accessLogId}` - Registrar salida

### Hardware Integration
- `GET /phidget-test/status` - Estado del Phidget
- `POST /phidget-test/initialize` - Inicializar Phidget
- `POST /phidget-test/relay/{channel}/toggle` - Toggle relay
- `POST /phidget-test/close` - Cerrar Phidget

## 📝 Notas Importantes

1. **Autenticación requerida:** Todos los endpoints excepto `/auth/login` y `/auth/health` requieren token JWT
2. **Formato de fechas:** ISO 8601 format (`2025-07-27T11:52:56`)
3. **Estados de visita:** `DENTRO`, `FUERA`, `PENDIENTE`
4. **IDs relacionales:** Muchos objetos tienen referencias a otros (visitorId, vehicleId, addressId, etc.)
5. **Campos opcionales:** Muchos campos pueden ser `null` (ej: `representativePhone`, `comments`)

## 🚀 Estado Actual del Sistema

- ✅ API funcionando en `localhost:5134`
- ✅ Usuario admin autenticado
- ✅ 35 marcas de vehículos disponibles
- ✅ 22 colores de vehículos disponibles
- ✅ 14 tipos de vehículos disponibles
- ✅ 10 casas registradas
- ✅ 5 notas en bitácora
- ✅ 3 pre-registros pendientes
- ✅ 7 logs de acceso (todos fuera)
- ✅ 0 visitas activas

---

*Documento generado el 18 de agosto de 2025 basado en pruebas reales del API*
