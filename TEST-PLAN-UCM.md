# Plan de Pruebas – Integración UCM (Grandstream 6301) y Telefonía

Este documento describe las pruebas recomendadas (unitarias, integración y end‑to‑end) para garantizar que la integración con el UCM6301 funcione correctamente, incluyendo: autenticación HTTP (challenge/login), listado de cuentas, CRUD de direcciones con extensiones, y el flujo de llamadas vía AMI.

## 1. Prerrequisitos
- UCM6301 accesible en la red (o mocks para pruebas automáticas).
- API MicroJack en ejecución con BD limpia o controlada:
  - Variable `MICROJACK_TEL_AMI_CONTEXT` si el contexto no es `from-internal`.
  - `appsettings.json` con `UpdateSettings` y CORS configurados para el entorno de pruebas.
- Credenciales UCM para API HTTP: usuario con permisos de API y AMI (si aplica).
- Certificado TLS self-signed permitido (la API está configurada para aceptarlo para `IUcmClient`).

## 2. Casos Unitarios
### 2.1 UcmClient (HTTP)
- Challenge OK: `GetChallengeAsync` devuelve `challenge` válido.
- Login OK: `LoginAsync` devuelve `cookie` válido a partir de `challenge+password` (MD5).
- Listado de cuentas OK: `ListAccountsAsync` devuelve lista con `extension/fullname/status/account_type`.
- Errores:
  - Challenge falla (HTTP 5xx, body inválido) → devuelve `error` y no `challenge`.
  - Login devuelve `status!=0` o sin `cookie` → `error`.
  - Listado devuelve `status!=0` o estructura inesperada → `error`.

### 2.2 TelephonyService (AMI)
- Extracción de host desde `BaseUrl`: soporta `https://host:8089/api`, `host:port`, `host`.
- Originate: si `Provider=Grandstream` con credenciales y `fromExtension`, marca `Ringing` (aceptación AMI).
- Fallback tech: si primer intento falla en `SIP`, intenta `PJSIP` (y viceversa).
- Estados y sellado de tiempos: `Pending → Initiated → Ringing → Completed/Failed` con `StartedAt/EndedAt` coherentes.
- Errores:
  - Config incompleta: sin `BaseUrl/Username/Password` / `fromExtension` → `Failed` + `ErrorMessage`.
  - AMI no accesible → `Failed` + `ErrorMessage`.

## 3. Casos de Integración (API)
Todos requieren JWT con los roles adecuados.

### 3.1 Configuración Telefonía
- `PUT /api/calls/settings` (Admin):
  - Body mínimo válido: `provider=Grandstream`, `baseUrl=https://<ip>:8089/api`, `username`, `password`, `defaultFromExtension`, `defaultTrunk=PJSIP`, `enabled=true`.
  - Validar respuesta: oculta `password` y persiste valores (ver `GET /api/calls/settings`).
- `GET /api/calls/settings` (Admin): devuelve configuración redactada correctamente.

### 3.2 UCM – Listado de cuentas (HTTP)
- `GET /api/calls/ucm/accounts` (Admin):
  - Con configuración correcta: devuelve `success=true`, `count>0` y `data[]` con extensiones.
  - Sin auth: `401`.
  - Config inválida (falta `BaseUrl` o `Username/Password`): `400`.
  - Error remoto (UCM caído): `5xx` con `ProblemDetails`.

### 3.3 CRUD Direcciones con Extensión
- Crear: `POST /api/addresses` (Admin)
  - Body: `{ identifier, extension, status?, message? }`.
  - Debe crear registro; `extension` única (índice único) y `identifier` actual mente única (política vigente).
  - Error si falta `identifier` o `extension`: `400`/`500` con mensaje claro.
  - Error si `extension` duplicada: `409`/`500` con mensaje claro.
- Listar: `GET /api/addresses` (auth)
  - Incluye `extension` en cada item; ordenado por `identifier`.
- Obtener: `GET /api/addresses/{id}` (auth)
  - Devuelve data con `identifier/extension/residents`.
- Actualizar: `PUT /api/addresses/{id}` (Admin)
  - Permite cambiar `identifier` y `extension` respetando unicidad.
  - Conflictos (`identifier` o `extension` en uso) → error controlado.
- Eliminar: `DELETE /api/addresses/{id}` (SuperAdmin)
  - No permite si tiene `residents` o `accessLogs` asociados; mensaje claro.

### 3.4 Flujo de llamadas (AMI)
- Crear llamada: `POST /api/calls` (Guard)
  - Body: `{ toNumber: <extension casa>, fromExtension?: <si falta usa default> }`.
  - Crea `CallRecord` con `Pending`→`Initiated`→`Ringing` y, para `Simulated`, llega a `Completed`.
  - Para `Grandstream`, al menos `Ringing` si AMI acepta el originate.
- Consultar llamada: `GET /api/calls/{id}` (Guard) devuelve `status` actualizado.
- Listar llamadas: `GET /api/calls?status=...&from=...&to=...` (Admin) devuelve ordenadas desc.
- Actualizar estado: `PATCH /api/calls/{id}/status` (Admin) permite `cancel/complete/fail` y setea `EndedAt`.
- Eliminar: `DELETE /api/calls/{id}` (SuperAdmin) borra el registro.

Notas:
- Hoy el sistema no consume eventos AMI (Newstate/Bridge/Hangup). Para E2E real, añadir un `HostedService` que procese eventos y cierre estados de forma automática.

## 4. Pruebas End‑to‑End Manuales (UCM real)
1) Configurar UCM:
   - Usuario API + AMI con permisos adecuados; AMI habilitado (TCP 5038) y accesible.
   - Confirmar contexto a usar (p.ej. `DLPN_DialPlan1`) y setear `MICROJACK_TEL_AMI_CONTEXT` si no es `from-internal`.
2) Configurar MicroJack:
   - `PUT /api/calls/settings` con credenciales del UCM y `enabled=true`.
3) Verificar cuentas UCM:
   - `GET /api/calls/ucm/accounts` (Admin) devuelve extensiones esperadas (incluidas casas).
4) Validar llamadas:
   - Elegir una casa: obtener su `extension` con `GET /api/addresses` o `/api/houses/buscar?q=<id>`.
   - `POST /api/calls` con `toNumber=<extension>` y `fromExtension=<ext guardia>`.
   - Observar estado en `GET /api/calls/{id}` y en el UCM (trazas/ CDR).
5) Forzar error:
   - Cambiar `baseUrl` o password a incorrecto y validar `Failed` con `ErrorMessage`.

## 5. Pruebas de Borde y Negativas
- Seguridad:
  - Acceso sin JWT: `401` en endpoints protegidos.
  - JWT rol `Guard` sin permisos de Admin para rutas de administración (`/calls/settings`, `/calls/ucm/accounts`) → `403`.
- Datos:
  - Crear/actualizar dirección con `extension` vacía → `400`.
  - Duplicar `extension` → error controlado (conflicto).
  - Eliminar dirección con dependencias → error controlado.
- Telefonía:
  - `fromExtension` inexistente: AMI debería rechazar; `CallRecord.Failed` con mensaje.
  - `defaultFromExtension` vacío y sin `fromExtension` en petición → `Failed` con mensaje claro.
- UCM HTTP:
  - Certificado inválido: (la API lo permite por defecto), en producción desactivar aceptación indiscriminada y validar cert.

## 6. Observabilidad y Logs
- Confirmar que se loguean eventos clave: challenge/login/listAccount, originate AMI (ActionID), errores y transiciones de estado.
- Sanitizar logs: no imprimir `password` ni `cookie`.

## 7. Backlog (Opcional / Futuro)
- Múltiples extensiones por casa:
  - Modelo `AddressExtension` (1:N con `Address`), endpoints CRUD (`/api/addresses/{id}/extensions`).
  - Endpoint de llamada por `addressId` que seleccione la extensión principal o por prioridad.
  - Tests adicionales: alta/baja de extensión, selección por prioridad, validación de unicidad global de extensiones.
- Listener de eventos AMI para actualizar estados en tiempo real (tests de eventos).
- Originate vía API HTTP del UCM (si se define el `action`/payload oficial) + tests.

## 8. Datos Semilla (verificación)
- El seeding actual crea direcciones con `extension` no vacía; verificar tras bootstrap que:
  - `GET /api/addresses` devuelve items con `identifier` y `extension`.
  - Búsqueda `/api/houses/buscar?q=...` incluye `Extension` en cada resultado.

---
Este plan cubre los puntos críticos para asegurar que el guardia pueda llamar a la casa usando su extensión registrada, que la configuración con UCM funcione (HTTP para cuentas, AMI para originación), y que el CRUD de direcciones con extensiones se comporte correctamente.

