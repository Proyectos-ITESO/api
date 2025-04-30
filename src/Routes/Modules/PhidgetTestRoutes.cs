// Routes/Modules/PhidgetTestRoutes.cs
using Microsoft.AspNetCore.Mvc;
using Phidget22;
using Phidget22.Events;

namespace MicroJack.API.Routes.Modules
{
    public static class PhidgetTestRoutes
    {
        private static readonly Dictionary<int, DigitalOutput> Relays = new();
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        public static void Configure(WebApplication app)
        {
            var phidgetTestGroup = app.MapGroup("/api/phidget-test")
                                     .WithTags("PhidgetTest");

            ConfigureInitializePhidget(phidgetTestGroup);
            ConfigureTestRelay(phidgetTestGroup);
            ConfigureGetRelayStatus(phidgetTestGroup);
            ConfigureClosePhidget(phidgetTestGroup);
        }

        private static void ConfigureInitializePhidget(RouteGroupBuilder group)
        {
            group.MapPost("/initialize", async (ILogger<Program> logger) =>
            {
                lock (_lock)
                {
                    if (_initialized)
                    {
                        return Results.Ok(new { message = "Phidget ya está inicializado" });
                    }

                    try
                    {
                        logger.LogInformation("Inicializando PhidgetInterfaceKit 0/0/4...");

                        // Inicializar los 4 relés
                        for (int i = 0; i < 4; i++)
                        {
                            var relay = new DigitalOutput();
                            relay.Channel = i;
                            
                            // Agregar manejadores de eventos para logging
                            relay.Attach += (sender, e) => {
                                logger.LogInformation($"Relé {((DigitalOutput)sender).Channel} conectado");
                            };
                            relay.Detach += (sender, e) => {
                                logger.LogInformation($"Relé {((DigitalOutput)sender).Channel} desconectado");
                            };
                            relay.Error += (sender, e) => {
                                logger.LogError($"Error en relé: {e.Description}");
                            };

                            relay.Open(5000); // 5000ms timeout
                            Relays[i] = relay;
                        }

                        _initialized = true;
                        logger.LogInformation("PhidgetInterfaceKit 0/0/4 inicializado exitosamente");

                        return Results.Ok(new { 
                            message = "Phidget inicializado exitosamente", 
                            relaysInitialized = Relays.Count 
                        });
                    }
                    catch (PhidgetException ex)
                    {
                        logger.LogError(ex, $"Error de Phidget: {ex.Description}");
                        return Results.Problem($"Error de Phidget: {ex.Description}", 
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error inesperado al inicializar Phidget");
                        return Results.Problem($"Error inesperado: {ex.Message}", 
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                }
            })
            .WithName("InitializePhidget")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static void ConfigureTestRelay(RouteGroupBuilder group)
        {
            group.MapPost("/relay/{channel}/toggle", async (ILogger<Program> logger, int channel) =>
            {
                if (!_initialized)
                {
                    return Results.BadRequest(new { message = "Phidget no está inicializado. Ejecuta /initialize primero." });
                }

                if (channel < 0 || channel > 3)
                {
                    return Results.BadRequest(new { message = "Canal inválido. Debe ser 0-3." });
                }

                try
                {
                    if (Relays.TryGetValue(channel, out var relay))
                    {
                        relay.State = !relay.State;
                        var newState = relay.State;
                        logger.LogInformation($"Relé {channel} cambiado a: {(newState ? "ON" : "OFF")}");

                        return Results.Ok(new { 
                            channel = channel, 
                            state = newState ? "ON" : "OFF",
                            success = true
                        });
                    }
                    else
                    {
                        return Results.NotFound(new { message = $"Relé {channel} no encontrado" });
                    }
                }
                catch (PhidgetException ex)
                {
                    logger.LogError(ex, $"Error de Phidget al cambiar estado del relé {channel}");
                    return Results.Problem($"Error de Phidget: {ex.Description}", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error inesperado al cambiar estado del relé {channel}");
                    return Results.Problem($"Error inesperado: {ex.Message}", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("ToggleRelay")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static void ConfigureGetRelayStatus(RouteGroupBuilder group)
        {
            group.MapGet("/status", (ILogger<Program> logger) =>
            {
                if (!_initialized)
                {
                    return Results.BadRequest(new { message = "Phidget no está inicializado. Ejecuta /initialize primero." });
                }

                try
                {
                    var relayStatus = new List<object>();
                    foreach (var kvp in Relays)
                    {
                        relayStatus.Add(new
                        {
                            channel = kvp.Key,
                            state = kvp.Value.State ? "ON" : "OFF",
                            attached = kvp.Value.Attached
                        });
                    }

                    return Results.Ok(new
                    {
                        initialized = _initialized,
                        relays = relayStatus
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error obteniendo estado de los relés");
                    return Results.Problem($"Error: {ex.Message}", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetPhidgetStatus")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static void ConfigureClosePhidget(RouteGroupBuilder group)
        {
            group.MapPost("/close", (ILogger<Program> logger) =>
            {
                if (!_initialized)
                {
                    return Results.BadRequest(new { message = "Phidget no está inicializado." });
                }

                try
                {
                    lock (_lock)
                    {
                        foreach (var relay in Relays.Values)
                        {
                            if (relay.Attached)
                            {
                                relay.State = false; // Apagar antes de cerrar
                                relay.Close();
                            }
                        }
                        Relays.Clear();
                        _initialized = false;
                    }

                    logger.LogInformation("Phidget cerrado exitosamente");
                    return Results.Ok(new { message = "Phidget cerrado exitosamente" });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error cerrando Phidget");
                    return Results.Problem($"Error: {ex.Message}", 
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("ClosePhidget")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}