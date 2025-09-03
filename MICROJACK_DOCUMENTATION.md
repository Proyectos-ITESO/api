# MicroJack API - Documentación Completa del Sistema

## 📋 Tabla de Contenidos

1. [Descripción General](#descripción-general)
2. [Sistema de Visitas e Identificación](#sistema-de-visitas-e-identificación)
3. [Sistema de Administración de Usuarios](#sistema-de-administración-de-usuarios)
4. [Sistema de Gestión de Vehículos](#sistema-de-gestión-de-vehículos)
5. [Sistema de Gestión de Visitantes](#sistema-de-gestión-de-visitantes)
6. [Sistema de Control de Acceso](#sistema-de-control-de-acceso)
7. [Sistema de Gestión de Archivos](#sistema-de-gestión-de-archivos)
8. [Autenticación y Autorización](#autenticación-y-autorización)
9. [Modelo de Datos](#modelo-de-datos)
10. [Endpoints Disponibles](#endpoints-disponibles)
11. [Guía de Uso](#guía-de-uso)

---

## 🏢 Descripción General

MicroJack API es un sistema integral de control de acceso para residencias privadas, desarrollado con ASP.NET Core 8.0 y C#. El sistema proporciona funcionalidades completas para:

- 🔐 **Control de acceso seguro** con autenticación JWT
- 📸 **Reconocimiento visual** con manejo de fotos (INE, rostro, placas)
- 🚗 **Gestión vehicular** con registro y seguimiento
- 👥 **Administración de usuarios** con roles y permisos granulares
- 📊 **Sistema de identificación de visitas** con búsqueda avanzada
- 🏠 **Gestión de residencias** y direcciones
- 📱 **Integración con hardware** (Phidgets para control de accesos)

---

## 🔍 Sistema de Visitas e Identificación

### Descripción
Sistema avanzado para identificar y trackear visitas, permitiendo búsquedas complejas por múltiples criterios y manteniendo historial completo de entradas y salidas.

### Funcionalidades Principales

#### 📅 Búsqueda por Fechas
- **Búsqueda por fecha específica**: Obtener todos los accesos de un día concreto
- **Búsqueda por rangos**: Filtrar accesos entre dos fechas
- **Historial temporal**: Ver el patrón de visitas a lo largo del tiempo

#### 👥 Búsqueda por Visitantes
- **Por nombre completo**: Buscar por nombre o parcialmente
- **Historial de visitante**: Ver todas las entradas/salidas de un visitante específico
- **Fotos integradas**: Incluye fotos rostro e INE en resultados

#### 🚗 Búsqueda por Vehículos
- **Por placa**: Búsqueda exacta o parcial de placas
- **Por características**: Filtrar por marca, color, tipo de vehículo
- **Historial vehicular**: Seguimiento completo por placa

#### 🏠 Búsqueda por Direcciones
- **Por identificador**: Buscar por número de casa o dirección
- **Historial por dirección**: Ver todas las visitas a una residencia específica

#### 🔍 Búsqueda Combinada Avanzada
```json
{
  "startDate": "2024-01-01T00:00:00",
  "endDate": "2024-12-31T23:59:59",
  "visitorName": "Juan",
  "licensePlate": "ABC",
  "brandId": 1,
  "colorId": 2,
  "status": "DENTRO",
  "page": 1,
  "pageSize": 50
}
```

### Endpoints

#### Búsqueda Específica
- `GET /api/accesslogs/by-date/{date}` - Accesos por fecha específica
- `GET /api/accesslogs/by-visitor/{visitorName}` - Accesos por nombre de visitante
- `GET /api/accesslogs/by-plate/{licensePlate}` - Accesos por placa
- `GET /api/accesslogs/by-vehicle?brandId=&colorId=&typeId=` - Accesos por características vehiculares
- `GET /api/accesslogs/by-address/{addressIdentifier}` - Accesos por dirección

#### Historial
- `GET /api/accesslogs/history/visitor/{visitorId}` - Historial completo de visitante
- `GET /api/accesslogs/history/vehicle/{licensePlate}` - Historial completo de vehículo
- `GET /api/accesslogs/history/address/{addressId}` - Historial completo de dirección

#### Búsqueda Avanzada
- `POST /api/accesslogs/search` - Búsqueda combinada con múltiples filtros y paginación

---

## 👥 Sistema de Administración de Usuarios

### Descripción
Sistema completo de gestión de usuarios con roles y permisos granulares, permitiendo un control detallado sobre quién puede hacer qué en el sistema.

### Jerarquía de Roles

#### 🔴 SuperAdmin
- **Acceso completo** al sistema
- Puede crear otros SuperAdmins
- Gestión total de roles y permisos
- Resetear contraseñas de cualquier usuario
- Activar/desactivar usuarios
- Eliminar cualquier usuario

#### 🟡 Admin
- **Gestión de usuarios**: Crear, ver, editar, eliminar guardias
- **Visibilidad completa**: Puede ver todos los roles y permisos
- **Búsqueda avanzada**: Buscar y ver información de usuarios
- **Operaciones del sistema**: Todas las funciones operativas
- **Gestión propia**: Cambiar su propia contraseña

#### 🔵 Guard
- **Operaciones básicas**: Registrar entradas y salidas
- **Información limitada**: Ver visitantes y vehículos autorizados
- **Control de acceso**: Operaciones esenciales de control de acceso
- **Gestión propia**: Cambiar su propia contraseña

### Endpoints de Administración

#### 📋 Gestión de Roles (`/api/admin/roles/`)
- `GET /api/admin/roles` - Listar todos los roles (AdminLevel)
- `GET /api/admin/roles/{id}` - Obtener rol por ID (AdminLevel)
- `POST /api/admin/roles` - Crear nuevo rol (SuperAdminLevel)
- `PUT /api/admin/roles/{id}` - Actualizar rol (SuperAdminLevel)
- `DELETE /api/admin/roles/{id}` - Eliminar rol (SuperAdminLevel)

#### 🔐 Gestión de Permisos por Rol
- `GET /api/admin/roles/{id}/permissions` - Ver permisos de rol (AdminLevel)
- `POST /api/admin/roles/{id}/permissions` - Agregar permiso a rol (SuperAdminLevel)
- `DELETE /api/admin/roles/{id}/permissions/{permission}` - Eliminar permiso (SuperAdminLevel)

#### 👥 Gestión de Usuarios-Roles
- `GET /api/admin/users/{guardId}/roles` - Ver roles de usuario (AdminLevel)
- `POST /api/admin/users/{guardId}/roles` - Asignar rol a usuario (SuperAdminLevel)
- `DELETE /api/admin/users/{guardId}/roles/{roleId}` - Remover rol de usuario (SuperAdminLevel)

#### 🔧 Gestión Avanzada de Usuarios
- `POST /api/admin/users/{guardId}/toggle-active?isActive=true/false` - Activar/Desactivar usuario (SuperAdminLevel)
- `POST /api/admin/users/{guardId}/reset-password` - Resetear contraseña (SuperAdminLevel)
- `GET /api/admin/users/{guardId}/permissions` - Ver permisos efectivos (AdminLevel)

#### 🔍 Utilidades
- `GET /api/admin/permissions` - Listar todos los permisos disponibles (AdminLevel)
- `GET /api/admin/users/search?searchTerm=` - Buscar usuarios (AdminLevel)

### Gestión de Contraseñas
- `POST /api/auth/change-password` - Cambiar contraseña propia
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/logout` - Cerrar sesión
- `GET /api/auth/me` - Obtener información del usuario actual

---

## 🚗 Sistema de Gestión de Vehículos

### Descripción
Sistema completo para la gestión de vehículos con campos opcionales y generación automática de datos para evitar conflictos en la base de datos.

### Características

#### 📝 Registro Simplificado
- **Campo obligatorio**: Solo la placa del vehículo es requerida
- **Campos opcionales**: Marca, color, tipo, URL de imagen de placa
- **Generación automática**: Si faltan datos opcionales, el sistema genera datos aleatorios

#### 🎯 Mock Data Automático
Cuando se registra un vehículo con solo la placa:
- **Marca**: Se asigna aleatoriamente de las marcas disponibles
- **Color**: Se asigna aleatoriamente de los colores disponibles  
- **Tipo**: Se asigna aleatoriamente de los tipos disponibles
- **Imagen**: Se genera URL placeholder para la placa

### Endpoints
- `GET /api/vehicles/` - Listar todos los vehículos
- `GET /api/vehicles/{id}` - Obtener vehículo por ID
- `POST /api/vehicles/` - Crear nuevo vehículo (solo placa requerida)
- `PUT /api/vehicles/{id}` - Actualizar vehículo
- `DELETE /api/vehicles/{id}` - Eliminar vehículo
- `GET /api/vehicles/search/{searchTerm}` - Buscar vehículos

### Ejemplo de Creación Mínima
```json
{
  "licensePlate": "ABC123"
}
```

El sistema automáticamente completará:
```json
{
  "licensePlate": "ABC123",
  "brandId": 3,
  "colorId": 5,
  "typeId": 2,
  "plateImageUrl": "/uploads/plates/abc123_placeholder.jpg"
}
```

---

## 👤 Sistema de Gestión de Visitantes

### Descripción
Sistema completo para el registro y gestión de visitantes con manejo de fotos y documentos de identificación.

### Características

#### 📸 Manejo de Fotos
- **Foto rostro**: Imagen facial del visitante
- **Foto INE**: Documento de identificación oficial
- **Almacenamiento seguro**: Las imágenes se guardan en el sistema de archivos
- **URLs en base de datos**: Solo se almacenan las rutas de acceso

#### 📝 Registro Completo
- **Información personal**: Nombre completo, teléfono, email
- **Documentación**: Número de identificación, tipo de documento
- ** Fotos integradas**: Asociación automática con imágenes subidas

### Endpoints
- `GET /api/visitors/` - Listar todos los visitantes
- `GET /api/visitors/{id}` - Obtener visitante por ID
- `POST /api/visitors/` - Crear nuevo visitante
- `PUT /api/visitors/{id}` - Actualizar visitante
- `DELETE /api/visitors/{id}` - Eliminar visitante
- `GET /api/visitors/search/{searchTerm}` - Buscar visitantes

---

## 🚪 Sistema de Control de Acceso

### Descripción
Sistema centralizado para el registro y gestión de accesos al residencial, con capacidades avanzadas de identificación y seguimiento.

### Características

#### 📋 Registro de Entradas
- **Registro completo**: Visitante, vehículo, dirección, guardia
- **Múltiples imágenes**: INE, rostro, placa en un solo registro
- **Estado tracking**: DENTRO, FUERA, PRE-REGISTRO
- **Timestamps precisos**: Fecha y hora exacta de entrada/salida

#### 🔍 Identificación Avanzada
- **Métodos de identificación**: INE, foto, placa, referencia
- **Niveles de confianza**: ALTO, MEDIO, BAJO
- **Palabras clave**: Búsqueda optimizada con términos relevantes

#### 📊 Registro de Salidas
- **Registro automático**: Timestamp de salida
- **Guardia responsable**: Quién registró la salida
- **Comentarios opcionales**: Notas sobre la salida

### Endpoints Principales

#### 📝 Registro de Accesos
- `GET /api/accesslogs/` - Listar todos los registros de acceso
- `GET /api/accesslogs/{id}` - Obtener registro por ID
- `GET /api/accesslogs/active` - Ver accesos activos (sin salida)
- `POST /api/accesslogs/` - Crear nuevo registro de acceso
- `PUT /api/accesslogs/{id}/exit` - Registrar salida

#### 🎯 Registro Unificado con Imágenes
- `POST /api/access/register-entry-with-images` - **Endpoint estrella**
  
Este endpoint permite:
```multipart-form-data
- visitorData: JSON con información del visitante
- vehicleData: JSON con información del vehículo  
- addressData: JSON con información de la dirección
- ineImage: Archivo de imagen INE
- faceImage: Archivo de imagen facial
- plateImage: Archivo de imagen de placa
```

El sistema crea automáticamente:
1. ✅ Nuevo visitante (si no existe)
2. ✅ Nuevo vehículo (si no existe)  
3. ✅ Nueva dirección (si no existe)
4. ✅ Registro de acceso completo
5. ✅ Asociación de todas las imágenes

---

## 📁 Sistema de Gestión de Archivos

### Descripción
Sistema robusto para la gestión de archivos multimedia con organización estructurada y serving estático.

### Estructura de Directorios

```
uploads/
├── faces/          # Fotos rostros
├── ine/            # Documentos INE
├── plates/         # Fotos de placas
└── temp/           # Archivos temporales
```

### Características

#### 📸 Subida de Archivos
- **Validación automática**: Tipos de archivo permitidos
- **Límites de tamaño**: Control de tamaño máximo por archivo
- **Nomenclatura estandarizada**: Nombres de archivos únicos y predecibles
- **URLs accesibles**: Las imágenes son accesibles vía HTTP

#### 🔍 Recuperación de Imágenes
- **Serving estático**: Configurado para servir imágenes directamente
- **URLs relativas**: Rutas fáciles de usar en aplicaciones frontend
- **Integración con modelos**: Las URLs se almacenan en los modelos correspondientes

### Endpoints
- `POST /api/upload/single` - Subir archivo individual
- `POST /api/upload/multiple` - Subir múltiples archivos
- `GET /uploads/{type}/{filename}` - Acceder a archivos subidos

---

## 🔐 Autenticación y Autorización

### Descripción
Sistema de seguridad robusto con JWT y control de acceso basado en roles (RBAC).

### Flujo de Autenticación

#### 1. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

#### 2. Respuesta
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "admin",
    "roles": ["SuperAdmin"]
  }
}
```

#### 3. Uso del Token
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Niveles de Autorización

#### 🔴 SuperAdminLevel
- Acceso a todos los endpoints del sistema
- Puede crear otros SuperAdmins
- Gestión completa de roles y permisos

#### 🟡 AdminLevel  
- Acceso a endpoints de gestión
- Puede crear y administrar usuarios
- No puede gestionar roles

#### 🔵 GuardLevel
- Acceso a endpoints operativos
- Puede registrar accesos y ver información básica
- Acceso limitado a funciones administrativas

### Políticas de Seguridad

- ✅ **Password Hashing**: BCrypt para almacenamiento seguro
- ✅ **JWT Tokens**: Tokens firmados con expiración configurable
- ✅ **Role Validation**: Verificación de roles en cada request
- ✅ **Permission Checks**: Validación granular de permisos
- ✅ **CORS Configurado**: Restricciones de origen configurables

---

## 🗃️ Modelo de Datos

### Entidades Principales

#### 👤 Guard (Usuario del Sistema)
```csharp
public class Guard
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> RoleNames { get; set; }
}
```

#### 👥 Visitor
```csharp
public class Visitor
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string IdentificationNumber { get; set; }
    public string? FaceImageUrl { get; set; }
    public string? IneImageUrl { get; set; }
}
```

#### 🚗 Vehicle
```csharp
public class Vehicle
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } // Obligatorio
    public int? BrandId { get; set; }        // Opcional
    public int? ColorId { get; set; }        // Opcional
    public int? TypeId { get; set; }         // Opcional
    public string? PlateImageUrl { get; set; }
}
```

#### 📋 AccessLog (Registro de Acceso)
```csharp
public class AccessLog
{
    public int Id { get; set; }
    public DateTime EntryTimestamp { get; set; }
    public DateTime? ExitTimestamp { get; set; }
    public string Status { get; set; } // DENTRO, FUERA, PRE-REGISTRO
    public int VisitorId { get; set; }
    public int? VehicleId { get; set; }
    public int AddressId { get; set; }
    public int EntryGuardId { get; set; }
    public int? ExitGuardId { get; set; }
    public string? IdentificationMethod { get; set; }
    public string? ConfidenceLevel { get; set; }
    public string? SearchKeywords { get; set; }
}
```

#### 🏠 Address
```csharp
public class Address
{
    public int Id { get; set; }
    public string Identifier { get; set; } // Casa 1, Depto 2A, etc.
    public string Street { get; set; }
    public int? ResidentId { get; set; }
}
```

#### 🔐 Role y Permission
```csharp
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Permissions { get; set; } // JSON string
}

public enum Permission
{
    ViewAccessLogs = 1,
    CreateAccessLog = 2,
    // ... 50+ permisos disponibles
}
```

### Catálogos del Sistema

#### Marcas de Vehículos
- Toyota, Honda, Nissan, Ford, Chevrolet, etc.

#### Colores de Vehículos  
- Blanco, Negro, Rojo, Azul, Gris, Plateado, etc.

#### Tipos de Vehículos
- Sedan, SUV, Pickup, Hatchback, Deportivo, etc.

#### Razones de Visita
- Familia, Amigos, Entrega, Mantenimiento, Emergencia, etc. (70+ opciones)

---

## 🌐 Endpoints Disponibles

### Autenticación (`/api/auth/`)
- `POST /login` - Iniciar sesión
- `POST /logout` - Cerrar sesión  
- `POST /change-password` - Cambiar contraseña
- `GET /me` - Obtener información del usuario actual
- `GET /health` - Verificar estado del sistema

### Administración (`/api/admin/`)
- `GET /roles` - Listar roles
- `POST /roles` - Crear rol
- `GET /roles/{id}` - Obtener rol
- `PUT /roles/{id}` - Actualizar rol
- `DELETE /roles/{id}` - Eliminar rol
- `GET /roles/{id}/permissions` - Ver permisos de rol
- `POST /roles/{id}/permissions` - Agregar permiso a rol
- `DELETE /roles/{id}/permissions/{permission}` - Eliminar permiso
- `GET /users/{guardId}/roles` - Ver roles de usuario
- `POST /users/{guardId}/roles` - Asignar rol a usuario
- `DELETE /users/{guardId}/roles/{roleId}` - Remover rol de usuario
- `POST /users/{guardId}/toggle-active` - Activar/desactivar usuario
- `POST /users/{guardId}/reset-password` - Resetear contraseña
- `GET /users/{guardId}/permissions` - Ver permisos de usuario
- `GET /permissions` - Listar todos los permisos
- `GET /users/search` - Buscar usuarios

### Usuarios/Guardias (`/api/guards/`)
- `GET /` - Listar guardias
- `GET /{id}` - Obtener guardia
- `POST /` - Crear guardia
- `PUT /{id}` - Actualizar guardia
- `DELETE /{id}` - Eliminar guardia

### Visitantes (`/api/visitors/`)
- `GET /` - Listar visitantes
- `GET /{id}` - Obtener visitante
- `POST /` - Crear visitante
- `PUT /{id}` - Actualizar visitante
- `DELETE /{id}` - Eliminar visitante
- `GET /search/{searchTerm}` - Buscar visitantes

### Vehículos (`/api/vehicles/`)
- `GET /` - Listar vehículos
- `GET /{id}` - Obtener vehículo
- `POST /` - Crear vehículo
- `PUT /{id}` - Actualizar vehículo
- `DELETE /{id}` - Eliminar vehículo
- `GET /search/{searchTerm}` - Buscar vehículos

### Registros de Acceso (`/api/accesslogs/`)
- `GET /` - Listar accesos
- `GET /{id}` - Obtener acceso
- `GET /active` - Accesos activos
- `POST /` - Crear acceso
- `PUT /{id}/exit` - Registrar salida
- `DELETE /{id}` - Eliminar acceso
- `GET /by-date/{date}` - Accesos por fecha
- `GET /by-visitor/{visitorName}` - Accesos por visitante
- `GET /by-plate/{licensePlate}` - Accesos por placa
- `GET /by-vehicle` - Accesos por características vehiculares
- `GET /by-address/{addressIdentifier}` - Accesos por dirección
- `GET /history/visitor/{visitorId}` - Historial de visitante
- `GET /history/vehicle/{licensePlate}` - Historial de vehículo
- `GET /history/address/{addressId}` - Historial de dirección
- `POST /search` - Búsqueda avanzada combinada

### Acceso Unificado (`/api/access/`)
- `POST /register-entry-with-images` - **Registro completo con imágenes**

### Archivos (`/api/upload/`)
- `POST /single` - Subir archivo individual
- `POST /multiple` - Subir múltiples archivos

### Direcciones (`/api/addresses/`)
- `GET /` - Listar direcciones
- `GET /{id}` - Obtener dirección
- `POST /` - Crear dirección
- `PUT /{id}` - Actualizar dirección
- `DELETE /{id}` - Eliminar dirección

### Catálogos (`/api/catalog/`)
- `GET /vehicle-brands` - Marcas de vehículos
- `GET /vehicle-colors` - Colores de vehículos
- `GET /vehicle-types` - Tipos de vehículos
- `GET /visit-reasons` - Razones de visita

---

## 🚀 Guía de Uso

### 1. Configuración Inicial

#### Primer Inicio
El sistema crea automáticamente:
- ✅ Usuario `admin` con contraseña `admin123`
- ✅ Roles predefinidos (SuperAdmin, Admin, Guard)
- ✅ Catálogos completos (marcas, colores, tipos, razones)
- ✅ Base de datos cifrada con SQLCipher

#### Variables de Entorno
```bash
# Directorio de datos (opcional)
export MICROJACK_DATA_DIR="/path/to/data"

# Configuración de la base de datos
# (se genera automáticamente si no existe)
```

### 2. Flujo de Operación Típico

#### Registro de Nueva Visita
```bash
# 1. Obtener token de autenticación
TOKEN=$(curl -s -X POST http://localhost:5134/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' | \
  grep -o '"token":"[^"]*' | sed 's/"token":"//')

# 2. Registrar entrada con 3 imágenes
curl -X POST http://localhost:5134/api/access/register-entry-with-images \
  -H "Authorization: Bearer $TOKEN" \
  -F 'visitorData={"fullName":"Juan Pérez","phoneNumber":"5551234567","identificationNumber":"JUAN123456"}' \
  -F 'vehicleData={"licensePlate":"ABC123"}' \
  -F 'addressData={"identifier":"Casa 15","street":"Calle Principal"}' \
  -F 'ineImage=@/path/to/ine.jpg' \
  -F 'faceImage=@/path/to/face.jpg' \
  -F 'plateImage=@/path/to/plate.jpg'
```

#### Búsqueda de Visitas
```bash
# Búsqueda por placa
curl -X GET http://localhost:5134/api/accesslogs/by-plate/ABC \
  -H "Authorization: Bearer $TOKEN"

# Búsqueda avanzada combinada
curl -X POST http://localhost:5134/api/accesslogs/search \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "startDate": "2024-01-01T00:00:00",
    "licensePlate": "ABC",
    "status": "DENTRO",
    "page": 1,
    "pageSize": 10
  }'
```

#### Administración de Usuarios
```bash
# Crear nuevo guardia
curl -X POST http://localhost:5134/api/guards/ \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName":"Carlos Guardia",
    "username":"carlos",
    "password":"Guardia123!"
  }'

# Asignar rol a usuario (SuperAdmin)
curl -X POST http://localhost:5134/api/admin/users/2/roles \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"roleId": 2}'
```

### 3. Mejores Prácticas

#### Seguridad
- 🔐 **Cambiar contraseña predeterminada** del usuario admin
- 🔑 **Usar contraseñas fuertes** para todos los usuarios
- 🛡️ **Asignar roles mínimos necesarios** (principio de menor privilegio)
- 📝 **Auditar regularmente** los accesos y permisos

#### Rendimiento
- 📸 **Optimizar imágenes** antes de subirlas
- 🔍 **Usar búsqueda específica** en lugar de obtener todos los registros
- 📊 **Implementar paginación** en consultas grandes
- 💾 **Limpiar archivos temporales** regularmente

#### Mantenimiento
- 🔄 **Reiniciar el servidor** después de actualizaciones
- 📋 **Monitorear logs** del sistema
- 💿 **Realizar backups** regulares de la base de datos
- 🆙 **Mantener actualizado** el sistema

### 4. Integración con Frontend

#### Autenticación
```javascript
// Login
const response = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username, password })
});

const { token, user } = await response.json();
localStorage.setItem('token', token);
```

#### Registro con Imágenes
```javascript
const formData = new FormData();
formData.append('visitorData', JSON.stringify(visitorData));
formData.append('vehicleData', JSON.stringify(vehicleData));
formData.append('addressData', JSON.stringify(addressData));
formData.append('ineImage', ineFile);
formData.append('faceImage', faceFile);
formData.append('plateImage', plateFile);

const response = await fetch('/api/access/register-entry-with-images', {
  method: 'POST',
  headers: { 'Authorization': `Bearer ${token}` },
  body: formData
});
```

#### Búsqueda Avanzada
```javascript
const searchParams = {
  startDate: '2024-01-01T00:00:00',
  visitorName: 'Juan',
  licensePlate: 'ABC',
  page: 1,
  pageSize: 20
};

const response = await fetch('/api/accesslogs/search', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(searchParams)
});
```

---

## 🛠️ Desarrollo y Testing

### Scripts Disponibles

#### Testing del Sistema
```bash
# Probar sistema de identificación de visitas
./test_visit_identification.sh

# Probar endpoints de administración
./test_admin_endpoints.sh

# Pruebas completas del sistema
bash test_complete_system.sh
```

#### Compilación y Ejecución
```bash
# Compilar proyecto
dotnet build

# Ejecutar en desarrollo
dotnet run

# Ejecutar con entorno específico
dotnet run --environment Development

# Compilar para producción
dotnet build --configuration Release
```

### Estructura del Proyecto

```
src/
├── Controllers/           # Controladores MVC tradicionales
├── Data/                 # Contexto de base de datos
├── Middleware/           # Middleware personalizado
├── Models/               # Modelos de datos
│   ├── Catalog/         # Entidades de catálogo
│   ├── Core/            # Entidades principales
│   ├── Enums/           # Enumeraciones
│   ├── Transaction/     # Entidades transaccionales
│   └── Ucm/             # Modelos UCM
├── Routes/              # Definición de endpoints
│   └── Modules/         # Módulos de rutas
├── Services/            # Lógica de negocio
│   └── Interfaces/      # Interfaces de servicios
└── Services/            # Implementación de servicios
```

---

## 📈 Características Técnicas

### Arquitectura
- **ASP.NET Core 8.0**: Framework moderno y de alto rendimiento
- **Entity Framework Core**: ORM para acceso a datos
- **SQLite con SQLCipher**: Base de datos cifrada
- **JWT Authentication**: Tokens seguros para autenticación
- **Minimal API**: Enfoque moderno para APIs
- **Dependency Injection**: Inyección de dependencias integrada

### Seguridad
- **Password Hashing**: BCrypt.NET-Next para hashing seguro
- **JWT Tokens**: Tokens firmados con expiración configurable
- **Role-Based Access Control**: Control de acceso por roles
- **CORS Configuration**: Restricciones de origen
- **Input Validation**: Validación automática de modelos
- **SQL Injection Protection**: Protección mediante Entity Framework

### Rendimiento
- **Async/Await**: Operaciones asíncronas en todo el sistema
- **Connection Pooling**: Pooling de conexiones a base de datos
- **Static File Serving**: Serving optimizado de archivos estáticos
- **Response Caching**: Caching configurado donde aplica
- **Lazy Loading**: Carga eficiente de datos relacionados

### Escalabilidad
- **Modular Architecture**: Sistema modular y extensible
- **Service Pattern**: Separación clara de responsabilidades
- **Repository Pattern**: Abstracción de acceso a datos
- **Configuration Management**: Configuración flexible
- **Logging System**: Logging estructurado y configurable

---

## 🔮 Roadmap Futuro

### Características Planeadas

#### 📱 Móvil
- **Aplicación móvil nativa** para guardias
- **Notificaciones push** para eventos importantes
- **Offline mode** para áreas sin conexión

#### 🤖 IA y Reconocimiento
- **Reconocimiento facial automático**
- **OCR para lectura automática de placas**
- **Análisis de patrones de visita**
- **Detección de anomalías**

#### 📊 Analytics y Reportes
- **Dashboard en tiempo real**
- **Reportes personalizados**
- **Exportación a múltiples formatos**
- **Métricas de seguridad**

#### 🔌 Integraciones
- **WhatsApp Business API** para notificaciones
- **Sistemas de CCTV existentes**
- **Sistemas de gestión de comunidades**
- **APIs de gobierno para verificación**

#### 🌐 Multi-tenancy
- **Soporte para múltiples residencias**
- **Gestión centralizada**
- **Roles por residencia**
- **Reportes consolidados**

---

## 📞 Soporte

### Documentación Adicional
- **CLAUDE.md**: Guía de desarrollo y configuración
- **TEST-PLAN-UCM.md**: Plan de pruebas UCM
- **API Documentation**: Documentación interactiva de API (Swagger/OpenAPI)

### Contacto
Para soporte técnico, reporte de bugs o solicitudes de características:
- **Issues**: Crear issue en el repositorio del proyecto
- **Email**: Contactar al equipo de desarrollo
- **Documentación**: Revisar archivos de documentación en el repositorio

---

## 📄 Licencia

Este proyecto es propiedad de MicroJack Systems y está protegido por leyes de propiedad intelectual. El uso, distribución o modificación no autorizada está prohibida.

© 2024 MicroJack Systems. Todos los derechos reservados.