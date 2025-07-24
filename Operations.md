# MicroJack PRO API - Complete Operations Guide

This document lists **ALL** the functions and operations available in the MicroJack PRO API for gate access control. The API provides comprehensive functionality for managing guards, visitors, vehicles, residents, access logs, and system catalogs.

## 🔐 Authentication & Authorization

### Authentication Endpoints

#### `POST /api/auth/login`
**Purpose:** Authenticate a guard and receive JWT token
**Authorization:** None required
**Request Body:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```
**Response:**
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

#### `POST /api/auth/logout`
**Purpose:** Logout current authenticated guard
**Authorization:** JWT token required
**Response:**
```json
{
  "success": true,
  "message": "Successfully logged out"
}
```

#### `POST /api/auth/change-password`
**Purpose:** Change password for current authenticated guard
**Authorization:** JWT token required
**Request Body:**
```json
{
  "currentPassword": "oldpassword",
  "newPassword": "newpassword123"
}
```

#### `GET /api/auth/me`
**Purpose:** Get current authenticated guard information
**Authorization:** JWT token required
**Response:**
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

#### `GET /api/auth/health`
**Purpose:** Check authentication service health and available policies
**Authorization:** None required
**Response:**
```json
{
  "success": true,
  "message": "Authentication service is healthy",
  "timestamp": "2025-07-24T20:06:50.5190443Z",
  "policies": [
    "GuardLevel: Guard, Admin, SuperAdmin",
    "AdminLevel: Admin, SuperAdmin",
    "SuperAdminLevel: SuperAdmin"
  ]
}
```

### Authorization Levels
- **GuardLevel**: Guards, Admins, and SuperAdmins can access
- **AdminLevel**: Only Admins and SuperAdmins can access
- **SuperAdminLevel**: Only SuperAdmins can access

---

## 👮 Guard Management

### Guard Operations

#### `GET /api/guards`
**Purpose:** Get all guards in the system
**Authorization:** AdminLevel required
**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "fullName": "Super Administrador",
      "username": "admin",
      "isActive": true,
      "lastLogin": null,
      "createdAt": "2025-07-24T20:06:41.6170946",
      "roles": [],
      "isAdmin": false
    }
  ]
}
```

#### `GET /api/guards/{id}`
**Purpose:** Get specific guard by ID
**Authorization:** AdminLevel required
**Parameters:** `id` (integer) - Guard ID
**Response:** Single guard object

#### `POST /api/guards`
**Purpose:** Create new guard
**Authorization:** AdminLevel required
**Request Body:**
```json
{
  "fullName": "New Guard Name",
  "username": "newguard",
  "password": "securepassword",
  "isActive": true
}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "id": 2,
    "fullName": "New Guard Name",
    "username": "newguard",
    "isActive": true,
    "createdAt": "2025-07-24T20:06:51.1600992Z"
  }
}
```

#### `PUT /api/guards/{id}`
**Purpose:** Update existing guard
**Authorization:** AdminLevel required
**Parameters:** `id` (integer) - Guard ID
**Request Body:**
```json
{
  "fullName": "Updated Name",
  "password": "newpassword", // Optional
  "isActive": false
}
```

#### `DELETE /api/guards/{id}`
**Purpose:** Delete guard (destructive operation)
**Authorization:** SuperAdminLevel required
**Parameters:** `id` (integer) - Guard ID
**Response:**
```json
{
  "success": true,
  "message": "Guard deleted successfully"
}
```

---

## 🏠 Address Management

### Address Operations

#### `GET /api/addresses`
**Purpose:** Get all addresses in the controlled area
**Authorization:** GuardLevel required
**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "streetAddress": "Calle Principal 123",
      "lotNumber": "A-01",
      "neighborhood": "Sector Norte",
      "city": "Ciudad",
      "zipCode": "12345",
      "isActive": true
    }
  ]
}
```

#### `GET /api/addresses/{id}`
**Purpose:** Get specific address by ID
**Authorization:** GuardLevel required
**Parameters:** `id` (integer) - Address ID

#### `POST /api/addresses`
**Purpose:** Create new address
**Authorization:** AdminLevel required
**Request Body:**
```json
{
  "streetAddress": "Calle Nueva 456",
  "lotNumber": "B-02",
  "neighborhood": "Sector Sur",
  "city": "Ciudad",
  "zipCode": "12346",
  "isActive": true
}
```

#### `PUT /api/addresses/{id}`
**Purpose:** Update existing address
**Authorization:** AdminLevel required
**Parameters:** `id` (integer) - Address ID

#### `DELETE /api/addresses/{id}`
**Purpose:** Delete address
**Authorization:** SuperAdminLevel required
**Parameters:** `id` (integer) - Address ID

---

## 🏘️ Resident Management

### Resident Operations

#### `GET /api/residents`
**Purpose:** Get all residents
**Authorization:** GuardLevel required
**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "fullName": "Juan Pérez",
      "email": "juan@example.com",
      "phone": "555-1234",
      "phoneExtension": "101",
      "addressId": 1,
      "isActive": true
    }
  ]
}
```

#### `GET /api/residents/{id}`
**Purpose:** Get specific resident by ID
**Authorization:** GuardLevel required
**Parameters:** `id` (integer) - Resident ID

#### `GET /api/residents/address/{addressId}`
**Purpose:** Get all residents living at specific address
**Authorization:** GuardLevel required
**Parameters:** `addressId` (integer) - Address ID

#### `POST /api/residents`
**Purpose:** Create new resident
**Authorization:** AdminLevel required
**Request Body:**
```json
{
  "fullName": "María González",
  "email": "maria@example.com",
  "phone": "555-5678",
  "phoneExtension": "102",
  "addressId": 1,
  "isActive": true
}
```

#### `PUT /api/residents/{id}`
**Purpose:** Update existing resident
**Authorization:** AdminLevel required
**Parameters:** `id` (integer) - Resident ID

#### `DELETE /api/residents/{id}`
**Purpose:** Delete resident
**Authorization:** SuperAdminLevel required
**Parameters:** `id` (integer) - Resident ID

---

## 👥 Visitor Management

### Visitor Operations

#### `GET /api/visitors`
**Purpose:** Get all visitors
**Authorization:** GuardLevel required
**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "fullName": "Carlos Visitante",
      "email": "carlos@example.com",
      "phone": "555-9999",
      "identificationNumber": "12345678",
      "identificationType": "DNI",
      "ineImageUrl": null,
      "faceImageUrl": null
    }
  ]
}
```

#### `GET /api/visitors/{id}`
**Purpose:** Get specific visitor by ID
**Authorization:** GuardLevel required
**Parameters:** `id` (integer) - Visitor ID

#### `POST /api/visitors`
**Purpose:** Create new visitor
**Authorization:** GuardLevel required
**Request Body:**
```json
{
  "fullName": "Ana Visitante",
  "email": "ana@example.com",
  "phone": "555-7777",
  "identificationNumber": "87654321",
  "identificationType": "Cedula",
  "ineImageUrl": "path/to/id/image.jpg",
  "faceImageUrl": "path/to/face/image.jpg"
}
```

#### `PUT /api/visitors/{id}`
**Purpose:** Update existing visitor
**Authorization:** GuardLevel required
**Parameters:** `id` (integer) - Visitor ID

#### `DELETE /api/visitors/{id}`
**Purpose:** Delete visitor
**Authorization:** AdminLevel required
**Parameters:** `id` (integer) - Visitor ID

---

## 🚗 Vehicle Management

### Vehicle Operations

#### `GET /api/vehicles`
**Purpose:** Get all vehicles
**Authorization:** GuardLevel required
**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "licensePlate": "ABC123",
      "plateImageUrl": "path/to/plate/image.jpg",
      "brandId": 1,
      "colorId": 5,
      "typeId": 2
    }
  ]
}
```

#### `GET /api/vehicles/{id}`
**Purpose:** Get specific vehicle by ID
**Authorization:** GuardLevel required
**Parameters:** `id` (integer) - Vehicle ID

#### `GET /api/vehicles/plate/{licensePlate}`
**Purpose:** Get vehicle by license plate number
**Authorization:** GuardLevel required
**Parameters:** `licensePlate` (string) - License plate number
**Example:** `/api/vehicles/plate/ABC123`

#### `POST /api/vehicles`
**Purpose:** Create new vehicle
**Authorization:** AdminLevel required
**Request Body:**
```json
{
  "licensePlate": "XYZ789",
  "plateImageUrl": "path/to/plate/image.jpg",
  "brandId": 2,
  "colorId": 3,
  "typeId": 1
}
```

#### `PUT /api/vehicles/{id}`
**Purpose:** Update existing vehicle
**Authorization:** GuardLevel required
**Parameters:** `id` (integer) - Vehicle ID

#### `DELETE /api/vehicles/{id}`
**Purpose:** Delete vehicle
**Authorization:** AdminLevel required
**Parameters:** `id` (integer) - Vehicle ID

---

## 📝 Access Log Management

### Access Log Operations

#### `GET /api/accesslogs`
**Purpose:** Get all access logs (entry/exit records)
**Authorization:** GuardLevel required
**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "visitorId": 1,
      "vehicleId": 1,
      "residentId": null,
      "purpose": "Business meeting",
      "entryTime": "2025-07-24T15:30:00Z",
      "exitTime": null,
      "isActive": true,
      "notes": "Authorized visit"
    }
  ]
}
```

#### `GET /api/accesslogs/{id}`
**Purpose:** Get specific access log by ID
**Authorization:** GuardLevel required
**Parameters:** `id` (integer) - Access log ID

#### `GET /api/accesslogs/active`
**Purpose:** Get all currently active visits (not yet exited)
**Authorization:** GuardLevel required
**Response:** List of access logs where `exitTime` is null

#### `POST /api/accesslogs`
**Purpose:** Create new access log entry (visitor arrives)
**Authorization:** GuardLevel required
**Request Body:**
```json
{
  "visitorId": 1,
  "vehicleId": 1,
  "residentId": 2,
  "purpose": "Family visit",
  "entryTime": "2025-07-24T16:00:00Z",
  "notes": "Regular visitor",
  "isActive": true
}
```

#### `PUT /api/accesslogs/{id}/exit`
**Purpose:** Mark visitor as exited (set exit time)
**Authorization:** GuardLevel required
**Parameters:** `id` (integer) - Access log ID
**Request Body:**
```json
{
  "exitTime": "2025-07-24T18:30:00Z",
  "exitNotes": "Normal departure"
}
```

#### `DELETE /api/accesslogs/{id}`
**Purpose:** Delete access log entry
**Authorization:** AdminLevel required
**Parameters:** `id` (integer) - Access log ID

---

## 📊 Catalog Management

### Vehicle Brand Catalog

#### `GET /api/catalogs/vehicle-brands`
**Purpose:** Get all vehicle brands (Toyota, Ford, BMW, etc.)
**Authorization:** None required
**Response:**
```json
{
  "success": true,
  "data": [
    {"id": 1, "name": "Toyota", "isActive": true},
    {"id": 2, "name": "Ford", "isActive": true},
    {"id": 3, "name": "BMW", "isActive": true}
    // ... 35+ brands total
  ]
}
```

### Vehicle Color Catalog

#### `GET /api/catalogs/vehicle-colors`
**Purpose:** Get all vehicle colors (Blanco, Negro, Rojo, etc.)
**Authorization:** None required
**Response:**
```json
{
  "success": true,
  "data": [
    {"id": 1, "name": "Blanco", "isActive": true},
    {"id": 2, "name": "Negro", "isActive": true},
    {"id": 3, "name": "Rojo", "isActive": true}
    // ... 22+ colors total
  ]
}
```

### Vehicle Type Catalog

#### `GET /api/catalogs/vehicle-types`
**Purpose:** Get all vehicle types (Sedán, SUV, Camioneta, etc.)
**Authorization:** None required
**Response:**
```json
{
  "success": true,
  "data": [
    {"id": 1, "name": "Sedán", "isActive": true},
    {"id": 2, "name": "SUV", "isActive": true},
    {"id": 3, "name": "Camioneta", "isActive": true}
    // ... 14+ types total
  ]
}
```

### Visit Reason Catalog

#### `GET /api/catalogs/visit-reasons`
**Purpose:** Get all visit reasons (Trabajo, Familia, Delivery, etc.)
**Authorization:** None required
**Response:**
```json
{
  "success": true,
  "data": [
    {"id": 1, "name": "Trabajo", "isActive": true},
    {"id": 2, "name": "Familia", "isActive": true},
    {"id": 3, "name": "Delivery", "isActive": true}
    // ... customizable reasons
  ]
}
```

---

## 🔧 Legacy Endpoints (Backward Compatibility)

### Registration Endpoints

#### `GET /api/registrations`
**Purpose:** Get all legacy registrations
**Authorization:** GuardLevel required

#### `GET /api/registrations/{id}`
**Purpose:** Get specific legacy registration
**Authorization:** GuardLevel required
**Parameters:** `id` (string) - Registration ID

#### `POST /api/registrations`
**Purpose:** Create new legacy registration
**Authorization:** GuardLevel required
**Request Body:**
```json
{
  "fullName": "Legacy User",
  "email": "legacy@example.com",
  "phone": "555-0000",
  "cotoId": "1",
  "registrationType": "Visitor",
  "house": "A-01",
  "visitReason": "Business",
  "visitorName": "John Doe",
  "visitedPerson": "Jane Smith",
  "status": "Pending"
}
```

### Pre-Registration Endpoints

#### `GET /api/preregistrations`
**Purpose:** Get all legacy pre-registrations
**Authorization:** GuardLevel required

#### `GET /api/preregistrations/by-plate/{plate}`
**Purpose:** Get pre-registration by license plate
**Authorization:** GuardLevel required
**Parameters:** `plate` (string) - License plate number

#### `GET /api/preregistrations/{id}/status`
**Purpose:** Get pre-registration status
**Authorization:** GuardLevel required
**Parameters:** `id` (string) - Pre-registration ID

#### `POST /api/preregistrations`
**Purpose:** Create new legacy pre-registration
**Authorization:** GuardLevel required
**Request Body:**
```json
{
  "fullName": "Pre-registered User",
  "email": "preuser@example.com",
  "phone": "555-1111",
  "plates": ["ABC123", "XYZ789"]
}
```

### Intermediate Registration Endpoints

#### `GET /api/intermediate`
**Purpose:** Get all intermediate registrations
**Authorization:** GuardLevel required

#### `GET /api/intermediate/pending`
**Purpose:** Get all pending intermediate registrations
**Authorization:** GuardLevel required

#### `GET /api/intermediate/cotos`
**Purpose:** Get available cotos for intermediate registration
**Authorization:** GuardLevel required

#### `POST /api/intermediate/approve/{token}`
**Purpose:** Approve intermediate registration by token
**Authorization:** AdminLevel required
**Parameters:** `token` (string) - Approval token

---

## 🚀 Unified Access Control (Frontend Optimized)

### Unified Entry Registration

#### `POST /api/access/register-entry`
**Purpose:** Register a complete entry with visitor, vehicle, and access log in one atomic operation
**Authorization:** GuardLevel required
**Request Body:**
```json
{
  "visitor": {
    "fullName": "María García",
    "email": "maria@example.com",
    "phone": "555-1234",
    "identificationNumber": "12345678",
    "identificationType": "DNI",
    "ineImageUrl": "/uploads/ine/20250724_143022_a1b2c3d4.jpg",
    "faceImageUrl": "/uploads/faces/20250724_143025_e5f6g7h8.jpg"
  },
  "vehicle": {
    "licensePlate": "ABC123",
    "plateImageUrl": "/uploads/plates/20250724_143030_i9j0k1l2.jpg",
    "brandId": 1,
    "colorId": 5,
    "typeId": 2
  },
  "residentId": 3,
  "purpose": "Family visit",
  "entryTime": "2025-07-24T16:00:00Z",
  "notes": "Regular visitor"
}
```
**Response:**
```json
{
  "success": true,
  "message": "Entry registered successfully",
  "data": {
    "accessLog": {
      "id": 15,
      "visitorId": 1,
      "vehicleId": 1,
      "residentId": 3,
      "purpose": "Family visit",
      "entryTime": "2025-07-24T16:00:00Z",
      "isActive": true
    },
    "visitor": { "id": 1, "fullName": "María García", ... },
    "vehicle": { "id": 1, "licensePlate": "ABC123", ... },
    "resident": { "id": 3, "fullName": "Juan Resident", ... }
  }
}
```

#### `POST /api/access/register-exit/{accessLogId}`
**Purpose:** Register visitor exit by access log ID
**Authorization:** GuardLevel required
**Parameters:** `accessLogId` (integer) - Access log ID
**Request Body:**
```json
{
  "exitTime": "2025-07-24T18:30:00Z",
  "exitNotes": "Normal departure"
}
```

#### `GET /api/access/active-visits`
**Purpose:** Get summary of all currently active visits
**Authorization:** GuardLevel required
**Response:**
```json
{
  "success": true,
  "count": 3,
  "data": [
    {
      "id": 15,
      "visitorName": "María García",
      "vehiclePlate": "ABC123",
      "purpose": "Family visit",
      "entryTime": "2025-07-24T16:00:00Z",
      "minutesInside": 45
    }
  ]
}
```

---

## 📁 File Upload Management

### Image Upload Operations

#### `POST /api/upload/image`
**Purpose:** Upload a single image file (face, INE, license plate, etc.)
**Authorization:** GuardLevel required
**Content-Type:** multipart/form-data
**Form Fields:**
- `file` (required): Image file (max 5MB, jpg/jpeg/png/gif/bmp)
- `category` (optional): Category for organization (faces, ine, plates, general)

**Response:**
```json
{
  "success": true,
  "message": "File uploaded successfully",
  "data": {
    "fileName": "original_photo.jpg",
    "savedFileName": "20250724_143022_a1b2c3d4.jpg",
    "url": "/uploads/faces/20250724_143022_a1b2c3d4.jpg",
    "category": "faces",
    "size": 2048576,
    "contentType": "image/jpeg",
    "uploadedAt": "2025-07-24T14:30:22Z"
  }
}
```

#### `POST /api/upload/images`
**Purpose:** Upload multiple image files at once (max 10 files)
**Authorization:** GuardLevel required
**Content-Type:** multipart/form-data
**Form Fields:**
- `files` (required): Array of image files
- `category` (optional): Category for all files

**Response:**
```json
{
  "success": true,
  "message": "Uploaded 3 files successfully",
  "data": {
    "uploaded": [
      {
        "fileName": "photo1.jpg",
        "savedFileName": "20250724_143022_a1b2c3d4.jpg",
        "url": "/uploads/general/20250724_143022_a1b2c3d4.jpg",
        "size": 1024768,
        "contentType": "image/jpeg"
      }
    ],
    "errors": [],
    "category": "general",
    "uploadedAt": "2025-07-24T14:30:22Z"
  }
}
```

#### `DELETE /api/upload/image`
**Purpose:** Delete an uploaded file
**Authorization:** AdminLevel required
**Query Parameters:**
- `fileUrl` (required): URL of file to delete

**Response:**
```json
{
  "success": true,
  "message": "File deleted successfully",
  "deletedUrl": "/uploads/faces/20250724_143022_a1b2c3d4.jpg"
}
```

---

## 📋 Event Log Management (Bitácora)

### Event Log Operations

#### `GET /api/eventlogs`
**Purpose:** Get all event logs with optional filters
**Authorization:** GuardLevel required
**Query Parameters:**
- `fromDate` (optional): Filter events from this date
- `toDate` (optional): Filter events to this date  
- `eventType` (optional): Filter by event type
- `guardId` (optional): Filter by guard ID

**Response:**
```json
{
  "success": true,
  "count": 25,
  "data": [
    {
      "id": 1,
      "eventType": "Security",
      "description": "Routine security check performed",
      "severity": "Info",
      "guardId": 1,
      "timestamp": "2025-07-24T14:30:00Z",
      "relatedEntityType": null,
      "relatedEntityId": null,
      "additionalData": null
    }
  ]
}
```

#### `GET /api/eventlogs/{id}`
**Purpose:** Get specific event log by ID
**Authorization:** GuardLevel required
**Parameters:** `id` (integer) - Event log ID

#### `POST /api/eventlogs`
**Purpose:** Create new event log entry
**Authorization:** GuardLevel required
**Request Body:**
```json
{
  "eventType": "Security",
  "description": "Unauthorized access attempt detected",
  "severity": "Warning",
  "relatedEntityType": "AccessLog",
  "relatedEntityId": 15,
  "additionalData": "{\"location\": \"Main gate\", \"camera\": \"CAM001\"}",
  "timestamp": "2025-07-24T14:30:00Z"
}
```

#### `GET /api/eventlogs/recent`
**Purpose:** Get recent event logs (last 24 hours by default)
**Authorization:** GuardLevel required
**Query Parameters:**
- `hours` (optional): Number of hours back (default: 24)

#### `GET /api/eventlogs/type/{eventType}`
**Purpose:** Get event logs filtered by event type
**Authorization:** GuardLevel required
**Parameters:** `eventType` (string) - Event type to filter

#### `GET /api/eventlogs/severity/{severity}`
**Purpose:** Get event logs filtered by severity level
**Authorization:** GuardLevel required
**Parameters:** `severity` (string) - Severity level (Info, Warning, Error, Critical)

#### `POST /api/eventlogs/quick`
**Purpose:** Create event log using predefined quick event types
**Authorization:** GuardLevel required
**Request Body:**
```json
{
  "quickEventType": "shift_start",
  "notes": "Started morning shift",
  "additionalData": "{\"shift\": \"morning\", \"position\": \"main_gate\"}"
}
```

**Available Quick Event Types:**
- `shift_start` - Guard shift started
- `shift_end` - Guard shift ended  
- `security_check` - Routine security check
- `maintenance` - Maintenance activity
- `incident` - Security incident
- `emergency` - Emergency situation
- `visitor_issue` - Visitor access issue
- `equipment_check` - Equipment status check
- `gate_malfunction` - Gate malfunction
- `unauthorized_access` - Unauthorized access attempt

#### `DELETE /api/eventlogs/{id}`
**Purpose:** Delete event log entry (admin only)
**Authorization:** AdminLevel required
**Parameters:** `id` (integer) - Event log ID

---

## 🔧 Hardware Integration

### Phidget Test Endpoints

#### `GET /api/phidget-test/status`
**Purpose:** Get current Phidget hardware status
**Authorization:** AdminLevel required
**Response:**
```json
{
  "success": true,
  "isConnected": false,
  "deviceInfo": null,
  "message": "No Phidget device connected (offline mode)"
}
```

#### `POST /api/phidget-test/initialize`
**Purpose:** Initialize Phidget hardware connection
**Authorization:** AdminLevel required

#### `POST /api/phidget-test/relay/{channel}/toggle`
**Purpose:** Toggle specific relay channel (gate control)
**Authorization:** GuardLevel required
**Parameters:** `channel` (integer) - Relay channel number (0-3)

#### `POST /api/phidget-test/close`
**Purpose:** Close Phidget hardware connection
**Authorization:** AdminLevel required

---

## 📋 Complete API Endpoint Summary

### Authentication (5 endpoints)
- `POST /api/auth/login` - Login and get JWT token
- `POST /api/auth/logout` - Logout current user
- `POST /api/auth/change-password` - Change user password
- `GET /api/auth/me` - Get current user info
- `GET /api/auth/health` - Check auth service health

### Guard Management (5 endpoints)
- `GET /api/guards` - List all guards
- `GET /api/guards/{id}` - Get guard by ID
- `POST /api/guards` - Create new guard
- `PUT /api/guards/{id}` - Update guard
- `DELETE /api/guards/{id}` - Delete guard

### Address Management (5 endpoints)
- `GET /api/addresses` - List all addresses
- `GET /api/addresses/{id}` - Get address by ID
- `POST /api/addresses` - Create new address
- `PUT /api/addresses/{id}` - Update address
- `DELETE /api/addresses/{id}` - Delete address

### Resident Management (6 endpoints)
- `GET /api/residents` - List all residents
- `GET /api/residents/{id}` - Get resident by ID
- `GET /api/residents/address/{addressId}` - Get residents by address
- `POST /api/residents` - Create new resident
- `PUT /api/residents/{id}` - Update resident
- `DELETE /api/residents/{id}` - Delete resident

### Visitor Management (5 endpoints)
- `GET /api/visitors` - List all visitors
- `GET /api/visitors/{id}` - Get visitor by ID
- `POST /api/visitors` - Create new visitor
- `PUT /api/visitors/{id}` - Update visitor
- `DELETE /api/visitors/{id}` - Delete visitor

### Vehicle Management (6 endpoints)
- `GET /api/vehicles` - List all vehicles
- `GET /api/vehicles/{id}` - Get vehicle by ID
- `GET /api/vehicles/plate/{licensePlate}` - Get vehicle by plate
- `POST /api/vehicles` - Create new vehicle
- `PUT /api/vehicles/{id}` - Update vehicle
- `DELETE /api/vehicles/{id}` - Delete vehicle

### Access Log Management (6 endpoints)
- `GET /api/accesslogs` - List all access logs
- `GET /api/accesslogs/{id}` - Get access log by ID
- `GET /api/accesslogs/active` - Get active visits
- `POST /api/accesslogs` - Create new access log
- `PUT /api/accesslogs/{id}/exit` - Mark visitor exit
- `DELETE /api/accesslogs/{id}` - Delete access log

### Catalog Management (4 endpoints)
- `GET /api/catalogs/vehicle-brands` - List vehicle brands
- `GET /api/catalogs/vehicle-colors` - List vehicle colors
- `GET /api/catalogs/vehicle-types` - List vehicle types
- `GET /api/catalogs/visit-reasons` - List visit reasons

### **🚀 Unified Access Control (3 endpoints - FRONTEND OPTIMIZED)**
- `POST /api/access/register-entry` - **Complete entry registration in one call**
- `POST /api/access/register-exit/{accessLogId}` - **Register visitor exit**
- `GET /api/access/active-visits` - **Get active visits summary**

### **📁 File Upload Management (3 endpoints)**
- `POST /api/upload/image` - **Upload single image (face, INE, plate)**
- `POST /api/upload/images` - **Upload multiple images**
- `DELETE /api/upload/image` - **Delete uploaded file**

### **📋 Event Log Management (8 endpoints - BITÁCORA)**
- `GET /api/eventlogs` - **List event logs with filters**
- `GET /api/eventlogs/{id}` - **Get specific event log**
- `POST /api/eventlogs` - **Create custom event log**
- `GET /api/eventlogs/recent` - **Get recent events**
- `GET /api/eventlogs/type/{eventType}` - **Filter by event type**
- `GET /api/eventlogs/severity/{severity}` - **Filter by severity**
- `POST /api/eventlogs/quick` - **Create quick event (10 predefined types)**
- `DELETE /api/eventlogs/{id}` - **Delete event log**

### Legacy Endpoints (11 endpoints)
- `GET /api/registrations` - List legacy registrations
- `GET /api/registrations/{id}` - Get legacy registration
- `POST /api/registrations` - Create legacy registration
- `GET /api/preregistrations` - List pre-registrations
- `GET /api/preregistrations/by-plate/{plate}` - Get pre-reg by plate
- `GET /api/preregistrations/{id}/status` - Get pre-reg status
- `POST /api/preregistrations` - Create pre-registration
- `GET /api/intermediate` - List intermediate registrations
- `GET /api/intermediate/pending` - List pending intermediate
- `GET /api/intermediate/cotos` - List available cotos
- `POST /api/intermediate/approve/{token}` - Approve intermediate

### Hardware Integration (4 endpoints)
- `GET /api/phidget-test/status` - Get hardware status
- `POST /api/phidget-test/initialize` - Initialize hardware
- `POST /api/phidget-test/relay/{channel}/toggle` - Control gate relay
- `POST /api/phidget-test/close` - Close hardware connection

## 🎯 Total API Capabilities

**📊 72 Total Endpoints** providing complete gate access control functionality:

### **🔥 New Frontend-Optimized Features:**
✅ **Unified Entry Registration** - Single call for visitor + vehicle + access log  
✅ **File Upload System** - Complete image management (5MB max, multiple formats)  
✅ **Bitácora System** - Comprehensive event logging with 10 quick event types  
✅ **Static File Serving** - Direct image access via URLs  
✅ **Atomic Operations** - Database consistency with rollback support

✅ **Complete CRUD Operations** for all entities
✅ **Role-Based Access Control** with 3 permission levels
✅ **JWT Authentication** with secure password management
✅ **Comprehensive Catalogs** with 70+ pre-populated entries
✅ **Legacy Compatibility** for existing systems
✅ **Hardware Integration** for physical gate control
✅ **Offline Operation** with encrypted local database
✅ **Real-Time Access Logging** with entry/exit tracking
✅ **Visitor & Vehicle Management** with full traceability
✅ **Resident & Address Management** with relationships

The API provides everything needed to run a complete, professional gate access control system with enterprise-level security and functionality.