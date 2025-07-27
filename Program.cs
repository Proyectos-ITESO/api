using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using MicroJack.API.Data;
using MicroJack.API.Services;
using MicroJack.API.Services.Interfaces;
using MicroJack.API.Routes;
using MicroJack.API.Models;
using MicroJack.API.Models.Catalog;
using MicroJack.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- 0. Configuración de Licencia ---
builder.Services.Configure<LicenseSettings>(builder.Configuration.GetSection("LicenseSettings"));
builder.Services.AddSingleton<ILicenseService, LicenseService>();
builder.Services.AddHttpClient();


// --- 1. Configuración de Servicios ---

// Añadir Logging básico
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

// Configurar SQLite encriptado con Entity Framework Core
var dbKey = MicroJack.API.Services.EncryptionService.GetOrCreateDatabaseKey();

// Get database path from configuration or use data directory
var dbPath = GetDatabasePath(builder.Configuration);
var connectionString = $"Data Source={dbPath};Password={dbKey}";

Console.WriteLine($"Using database path: {dbPath}");

static string GetDatabasePath(IConfiguration configuration)
{
    var configConnection = configuration.GetConnectionString("DefaultConnection");
    
    // If connection string specifies an absolute path, use it
    if (!string.IsNullOrEmpty(configConnection) && configConnection.Contains("Data Source="))
    {
        var dataSourcePart = configConnection.Split(';').FirstOrDefault(s => s.StartsWith("Data Source="));
        if (dataSourcePart != null)
        {
            var dbPath = dataSourcePart.Replace("Data Source=", "").Trim();
            if (Path.IsPathRooted(dbPath))
            {
                return dbPath;
            }
        }
    }
    
    // Use data directory for database
    var dataDir = Environment.GetEnvironmentVariable("MICROJACK_DATA_DIR");
    if (!string.IsNullOrEmpty(dataDir) && Directory.Exists(dataDir))
    {
        return Path.Combine(dataDir, "microjack.db");
    }
    
    // Use current working directory if writable, otherwise use user data directory
    var currentDir = Directory.GetCurrentDirectory();
    try
    {
        var testFile = Path.Combine(currentDir, ".write_test");
        File.WriteAllText(testFile, "test");
        File.Delete(testFile);
        return Path.Combine(currentDir, "microjack.db");
    }
    catch
    {
        // Current directory is not writable, use user data directory
        var userDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDataDir = Path.Combine(userDataDir, "MicroJack");
        Directory.CreateDirectory(appDataDir);
        return Path.Combine(appDataDir, "microjack.db");
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Configure JWT Authentication
var jwtSecret = builder.Configuration["JWT:Secret"] ?? "MicroJack-DefaultSecret-ChangeInProduction-2024";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = "Username"
    };
});

// Add Authorization services with policies
builder.Services.AddAuthorization(options =>
{
    // Admin-level policies (Admin or SuperAdmin can access)
    options.AddPolicy("AdminLevel", policy => 
        policy.RequireRole("Admin", "SuperAdmin"));
    
    // Guard-level policies (any authenticated user can access)
    options.AddPolicy("GuardLevel", policy =>
        policy.RequireRole("Guard", "Admin", "SuperAdmin"));
    
    // Super Admin only (highest privilege level)
    options.AddPolicy("SuperAdminLevel", policy =>
        policy.RequireRole("SuperAdmin"));
});

// Registrar servicios con Entity Framework Core

// Legacy services (mantener por compatibilidad)
// Legacy services removed - using new normalized entities

// Pre-registration service
builder.Services.AddScoped<IPreRegistrationService, PreRegistrationService>();

// Bitácora service
builder.Services.AddScoped<IBitacoraService, BitacoraService>();

// Core entity services
builder.Services.AddScoped<IGuardService, GuardService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IVisitorService, VisitorService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IResidentService, ResidentService>();

// Catalog services
builder.Services.AddScoped<ICatalogService<VehicleBrand>, VehicleBrandService>();
builder.Services.AddScoped<ICatalogService<VehicleColor>, VehicleColorService>();
builder.Services.AddScoped<ICatalogService<VehicleType>, VehicleTypeService>();
builder.Services.AddScoped<ICatalogService<VisitReason>, VisitReasonService>();

// Transaction services
builder.Services.AddScoped<IAccessLogService, AccessLogService>();
builder.Services.AddScoped<IEventLogService, EventLogService>();

// Authentication and authorization services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

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

// --- Validar Licencia ---
try
{
    var licenseService = app.Services.GetRequiredService<ILicenseService>();
    licenseService.ValidateLicense();
    app.Logger.LogInformation("Validación de licencia exitosa.");
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "La validación de la licencia falló. La aplicación se cerrará.");
    // Terminar la aplicación si la validación falla
    return; 
}


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

// Configure static file serving for uploads
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Add cache headers for uploaded images
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=86400"); // 24 hours
    }
});

// Habilitar CORS
app.UseCors();

// Enable Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// --- 3. Configuración de Rutas ---
app.Logger.LogInformation("Configurando endpoints...");
ApiRoutes.Configure(app);

// --- 4. Iniciar la Aplicación ---
app.Logger.LogInformation("Iniciando MicroJack.API...");
app.Run();