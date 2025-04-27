// Program.cs - MicroJack.API
using Microsoft.AspNetCore.Mvc; // Para FromBody, FromQuery, etc.
using Microsoft.Extensions.Options;
using MongoDB.Bson; // Para ObjectId
using System.Text.Json;
using MicroJack.API.Models;  // Asegúrate que el namespace sea correcto
using MicroJack.API.Services; // Asegúrate que el namespace sea correcto

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configuración de Servicios ---

// Añadir Logging básico
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

// Cargar configuración de MongoDbSettings desde appsettings.json
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Registrar MongoDbService como Singleton (una instancia para toda la app)
builder.Services.AddSingleton<MongoDbService>();

// Configurar CORS (Cross-Origin Resource Sharing) desde appsettings.json
var corsSettings = builder.Configuration.GetSection("CorsSettings");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (!allowedOrigins.Any())
        {
            Console.WriteLine("ADVERTENCIA: No hay orígenes CORS definidos en CorsSettings:AllowedOrigins. Permitiendo cualquiera para desarrollo.");
            // ¡PELIGROSO EN PRODUCCIÓN! Solo para desarrollo si no hay configuración.
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            Console.WriteLine($"CORS configurado para orígenes: {string.Join(", ", allowedOrigins)}");
        }
    });
});

// Habilitar servicios para OpenAPI/Swagger (documentación de API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "MicroJack.API", Version = "v1" });
});

// (Opcional) Añadir servicios de controladores si usas atributos como [ApiController] en el futuro
// builder.Services.AddControllers();

var app = builder.Build();

// --- 2. Configuración del Pipeline HTTP ---

// Habilitar Swagger UI solo en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MicroJack.API v1");
        // Mostrar Swagger UI en la raíz (/) para facilitar acceso en desarrollo
        c.RoutePrefix = string.Empty;
    });
    app.Logger.LogInformation("Swagger UI habilitado en la raíz (/)");
}

// Redirección HTTPS (importante para seguridad)
app.UseHttpsRedirection();

// Habilitar CORS (¡Debe ir antes de definir los endpoints!)
app.UseCors();

// --- 3. Definición de Endpoints (Minimal API) ---

app.Logger.LogInformation("Configurando endpoints...");

// --- Endpoints para Registros Principales ---
var registrationsApiGroup = app.MapGroup("/api/registrations")
                               .WithTags("Registrations"); // Agrupa en Swagger

// GET /api/registrations (con búsqueda opcional por 'search' - ajusta si usas 'plate')
registrationsApiGroup.MapGet("/", async (MongoDbService mongoService, ILogger<Program> logger, [FromQuery] string? search) =>
{
    // Nota: El servicio GetRegistrationsAsync ahora acepta 'search', no 'plate'. Ajusta si es necesario.
    logger.LogInformation("Recibida solicitud GET /api/registrations (Búsqueda: {Search})", search ?? "N/A");
    try
    {
        // Asume que GetRegistrationsAsync maneja el parámetro de búsqueda/placa
        var registrations = await mongoService.GetRegistrationsAsync(search);
        return Results.Ok(registrations);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error procesando GET /api/registrations");
        return Results.Problem("Ocurrió un error al obtener los registros.", statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetRegistrations")
.Produces<List<Registration>>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status500InternalServerError);

// POST /api/registrations (Crea un nuevo registro)
registrationsApiGroup.MapPost("/", async (MongoDbService mongoService, ILogger<Program> logger, [FromBody] Registration registration) =>
{
    logger.LogInformation("Recibida solicitud POST /api/registrations para visitante: {VisitorName}, Status: {Status}", registration.VisitorName, registration.Status ?? "N/A");

    // Validación básica (puedes mejorarla)
    if (string.IsNullOrWhiteSpace(registration.RegistrationType) ||
        string.IsNullOrWhiteSpace(registration.House) ||
        string.IsNullOrWhiteSpace(registration.VisitReason) ||
        string.IsNullOrWhiteSpace(registration.VisitorName) ||
        string.IsNullOrWhiteSpace(registration.VisitedPerson) ||
        // string.IsNullOrWhiteSpace(registration.Guard) || // Quitamos Guardia si ya no se usa
        string.IsNullOrWhiteSpace(registration.Status)) // Validar que el Status venga
    {
        logger.LogWarning("Solicitud POST inválida: Faltan campos requeridos (incluyendo Status).");
        return Results.ValidationProblem(new Dictionary<string, string[]> {
            {"RequestBody", new[] { "Faltan campos requeridos (registrationType, house, visitReason, visitorName, visitedPerson, status)." }}
        });
    }

    try
    {
        // El servicio CreateRegistrationAsync ya NO asigna Status, lo recibe del frontend
        var createdRegistration = await mongoService.CreateRegistrationAsync(registration);
        return Results.CreatedAtRoute("GetRegistrationById", new { id = createdRegistration.Id }, createdRegistration);
    }
    catch (ApplicationException appEx)
    {
         logger.LogError(appEx, "Error de aplicación al crear registro.");
         return Results.Problem(appEx.Message, statusCode: StatusCodes.Status400BadRequest); // Error controlado (ej. Folio duplicado)
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error inesperado procesando POST /api/registrations");
        return Results.Problem("Ocurrió un error inesperado al crear el registro.", statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("CreateRegistration")
.Produces<Registration>(StatusCodes.Status201Created)
.ProducesValidationProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError);

// GET /api/registrations/{id} (Obtiene un registro por su ID)
registrationsApiGroup.MapGet("/{id}", async (MongoDbService mongoService, ILogger<Program> logger, string id) =>
{
    logger.LogInformation("Recibida solicitud GET /api/registrations/{Id}", id);
    if (!ObjectId.TryParse(id, out _)) {
         return Results.BadRequest("El ID proporcionado no es un ObjectId válido.");
    }
    try
    {
        var registration = await mongoService.GetRegistrationByIdAsync(id);
        if (registration is null)
        {
            logger.LogWarning("Registro no encontrado para ID: {Id}", id);
            return Results.NotFound(new { message = $"Registro con ID {id} no encontrado." });
        }
        return Results.Ok(registration);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error procesando GET /api/registrations/{Id}", id);
        return Results.Problem("Ocurrió un error al obtener el registro.", statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetRegistrationById") // Nombre usado por CreatedAtRoute
.Produces<Registration>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status400BadRequest) // Por ID inválido
.ProducesProblem(StatusCodes.Status500InternalServerError);


// --- Endpoints para Pre-Registros ---
var preRegistrationsApiGroup = app.MapGroup("/api/preregistrations")
                                  .WithTags("PreRegistrations");

// POST /api/preregistrations (Crea un nuevo pre-registro)
preRegistrationsApiGroup.MapPost("/", async (MongoDbService mongoService, ILogger<Program> logger, [FromBody] PreRegistration preRegistration) =>
{
    logger.LogInformation("Recibida solicitud POST /api/preregistrations para placas: {Plates}", preRegistration.Plates);
    if (string.IsNullOrWhiteSpace(preRegistration.Plates))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> {
            {"plates", new[] { "Las placas son requeridas para el pre-registro." }}
        });
    }
    try
    {
        // El servicio asignará Status="PENDIENTE" y CreatedAt
        var createdPreReg = await mongoService.CreatePreRegistrationAsync(preRegistration);
        // Podrías añadir un endpoint GET /api/preregistrations/{id} si quieres que CreatedAtRoute funcione perfectamente
        return Results.Created($"/api/preregistrations/{createdPreReg.Id}", createdPreReg);
    }
    catch (ApplicationException appEx) 
    { 
        logger.LogError(appEx, "Error de aplicación al crear pre-registro.");
        return Results.Problem(appEx.Message, statusCode: StatusCodes.Status400BadRequest);
    }
    catch (Exception ex) 
    { 
        logger.LogError(ex, "Error inesperado procesando POST /api/preregistrations");
        return Results.Problem("Error inesperado.", statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("CreatePreRegistration")
.Produces<PreRegistration>(StatusCodes.Status201Created)
.ProducesValidationProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError);

// GET /api/preregistrations/by-plate/{plate} (Busca pre-registro PENDIENTE por placa)
preRegistrationsApiGroup.MapGet("/by-plate/{plate}", async (MongoDbService mongoService, ILogger<Program> logger, string plate) =>
{
    logger.LogInformation("Recibida solicitud GET /api/preregistrations/by-plate/{Plate}", plate);
    if (string.IsNullOrWhiteSpace(plate)) 
    { 
        return Results.BadRequest("El parámetro 'plate' es requerido."); 
    }
    try
    {
        var preRegistration = await mongoService.GetPendingPreRegistrationByPlateAsync(plate);
        if (preRegistration == null)
        {
            logger.LogWarning("No se encontró pre-registro PENDIENTE para placa: {Plate}", plate);
            return Results.NotFound(new { message = $"No se encontró pre-registro pendiente para la placa {plate}." });
        }
        return Results.Ok(preRegistration);
    }
    catch (Exception ex) 
    { 
        logger.LogError(ex, "Error procesando GET /api/preregistrations/by-plate/{Plate}", plate);
        return Results.Problem("Error al buscar.", statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetPreRegistrationByPlate")
.Produces<PreRegistration>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError);

// GET /api/preregistrations (Obtiene todos los pre-registros, con búsqueda opcional)
preRegistrationsApiGroup.MapGet("/", async (MongoDbService mongoService, ILogger<Program> logger, [FromQuery] string? search) =>
{
    logger.LogInformation("Recibida solicitud GET /api/preregistros (Búsqueda: {Search})", search ?? "N/A");
    try
    {
        var preRegistrations = await mongoService.GetPreRegistrationsAsync(search); // Llama al método del servicio
        return Results.Ok(preRegistrations);
    }
    catch (Exception ex) 
    { 
        logger.LogError(ex, "Error procesando GET /api/preregistrations");
        return Results.Problem("Error al obtener.", statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetPreRegistrations")
.Produces<List<PreRegistration>>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status500InternalServerError);

// PATCH /api/preregistrations/{id}/status (Actualiza el estado de un pre-registro)
// Define un DTO (Data Transfer Object) simple para el cuerpo de la solicitud PATCH
preRegistrationsApiGroup.MapPatch("/{id}/status",
    async (MongoDbService mongoService, ILogger<Program> logger, string id, [FromBody] UpdateStatusRequest request) =>
{
    logger.LogInformation("Recibida solicitud PATCH /api/preregistrations/{Id}/status a '{NewStatus}'", id, request?.NewStatus);

    if (string.IsNullOrWhiteSpace(request?.NewStatus)) 
    { 
        return Results.BadRequest("El campo 'newStatus' es requerido."); 
    }
    if (!ObjectId.TryParse(id, out _)) 
    { 
        return Results.BadRequest("El ID proporcionado no es un ObjectId válido."); 
    }

    try
    {
        bool success = await mongoService.UpdatePreRegistrationStatusAsync(id, request.NewStatus);
        if (success) 
        { 
            return Results.NoContent(); // 204 Éxito sin contenido
        }
        else
        {
            logger.LogWarning("No se pudo actualizar estado para pre-registro ID: {Id}. ¿No encontrado?", id);
            return Results.NotFound(new { message = $"No se pudo encontrar o actualizar el pre-registro con ID {id}." });
        }
    }
    catch (Exception ex) 
    { 
        logger.LogError(ex, "Error procesando PATCH /api/preregistrations/{Id}/status", id);
        return Results.Problem("Error al actualizar.", statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("UpdatePreRegistrationStatus")
.Produces(StatusCodes.Status204NoContent)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError);


// --- 4. Iniciar la Aplicación ---
app.Logger.LogInformation("Iniciando MicroJack.API...");
app.Run();

// Define el record fuera de los endpoints
public record UpdateStatusRequest(string NewStatus);