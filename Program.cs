// Program.cs - MicroJack.API
using Microsoft.Extensions.Options;
using MicroJack.API.Models;
using MicroJack.API.Services;
using MicroJack.API.Services.Interfaces;
using MicroJack.API.Routes;

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

// Registrar servicios de MongoDB
builder.Services.AddSingleton<IMongoService, BaseMongoService>();
builder.Services.AddSingleton<IRegistrationService, RegistrationService>();
builder.Services.AddSingleton<IPreRegistrationService, PreRegistrationService>();

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