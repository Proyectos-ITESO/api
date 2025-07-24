using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Services;
using MicroJack.API.Services.Interfaces;
using MicroJack.API.Routes;
using MicroJack.API.Models.Catalog;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configuración de Servicios ---

// Añadir Logging básico
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

// Configurar SQLite encriptado con Entity Framework Core
var dbKey = MicroJack.API.Services.EncryptionService.GetOrCreateDatabaseKey();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? $"Data Source=microjack.db;Password={dbKey}";

// Agregar la clave de encriptación si no está presente
if (!connectionString.Contains("Password="))
{
    connectionString += $";Password={dbKey}";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Registrar servicios con Entity Framework Core

// Legacy services (mantener por compatibilidad)
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IPreRegistrationService, PreRegistrationService>();
builder.Services.AddScoped<IIntermediateRegistrationService, IntermediateRegistrationService>();

// Core entity services
builder.Services.AddScoped<IGuardService, GuardService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IVisitorService, VisitorService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// Catalog services
builder.Services.AddScoped<ICatalogService<VehicleBrand>, VehicleBrandService>();
builder.Services.AddScoped<ICatalogService<VehicleColor>, VehicleColorService>();
builder.Services.AddScoped<ICatalogService<VehicleType>, VehicleTypeService>();
builder.Services.AddScoped<ICatalogService<VisitReason>, VisitReasonService>();

// Transaction services
builder.Services.AddScoped<IAccessLogService, AccessLogService>();
builder.Services.AddScoped<IEventLogService, EventLogService>();

// Other services
builder.Services.AddScoped<IPhidgetService, PhidgetService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<DatabaseInitializationService>();

// Configurar CORS
var corsSettings = builder.Configuration.GetSection("CorsSettings");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (!allowedOrigins.Any())
        {
            Console.WriteLine("ADVERTENCIA: No hay orígenes CORS definidos en CorsSettings:AllowedOrigins. Permitiendo cualquiera para desarrollo.");
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

// Habilitar servicios para OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "MicroJack.API", Version = "v1" });
});

var app = builder.Build();

// Crear la base de datos y aplicar migraciones automáticamente
using (var scope = app.Services.CreateScope())
{
    var dbInitService = scope.ServiceProvider.GetRequiredService<DatabaseInitializationService>();
    await dbInitService.InitializeAsync();
    app.Logger.LogInformation("Base de datos SQLite inicializada con datos de catálogo");
}

// --- 2. Configuración del Pipeline HTTP ---

// Habilitar Swagger UI solo en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MicroJack.API v1");
        c.RoutePrefix = string.Empty;
    });
    app.Logger.LogInformation("Swagger UI habilitado en la raíz (/)");
}

// Redirección HTTPS
app.UseHttpsRedirection();

// Habilitar CORS
app.UseCors();

// --- 3. Configuración de Rutas ---
app.Logger.LogInformation("Configurando endpoints...");
ApiRoutes.Configure(app);

// --- 4. Iniciar la Aplicación ---
app.Logger.LogInformation("Iniciando MicroJack.API...");
app.Run();