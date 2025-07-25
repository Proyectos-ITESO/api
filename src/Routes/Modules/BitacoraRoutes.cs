using Microsoft.AspNetCore.Mvc;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Routes.Modules
{
    public static class BitacoraRoutes
    {
        public static void MapBitacoraRoutes(this WebApplication app)
        {
            var group = app.MapGroup("/api/bitacora")
                          .WithTags("Bitácora")
                          .RequireAuthorization();

            // Crear nota en bitácora
            group.MapPost("/", async (
                [FromBody] CreateBitacoraRequest request,
                IBitacoraService bitacoraService,
                HttpContext context) =>
            {
                try
                {
                    var guardId = int.Parse(context.User.FindFirst("GuardId")?.Value ?? "1");

                    var note = new BitacoraNote
                    {
                        Note = request.Note,
                        GuardId = guardId
                    };

                    var created = await bitacoraService.CreateNoteAsync(note);

                    return Results.Created($"/api/bitacora/{created.Id}", new
                    {
                        success = true,
                        message = "Nota agregada a la bitácora",
                        data = new
                        {
                            created.Id,
                            created.Note,
                            created.Timestamp,
                            created.GuardId,
                            GuardName = created.Guard?.Username
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error creando nota en bitácora: {ex.Message}");
                }
            })
            .WithName("CreateBitacoraNote")
            .WithSummary("Crear una nota en la bitácora");

            // Obtener todas las notas (con filtros opcionales)
            group.MapGet("/", async (
                [FromQuery] int? guardId,
                [FromQuery] DateTime? fechaInicio,
                [FromQuery] DateTime? fechaFin,
                IBitacoraService bitacoraService) =>
            {
                try
                {
                    List<BitacoraNote> notes;

                    if (guardId.HasValue || fechaInicio.HasValue || fechaFin.HasValue)
                    {
                        notes = await bitacoraService.GetNotesFilteredAsync(guardId, fechaInicio, fechaFin);
                    }
                    else
                    {
                        notes = await bitacoraService.GetAllNotesAsync();
                    }

                    var result = notes.Select(n => new
                    {
                        n.Id,
                        n.Note,
                        n.Timestamp,
                        n.GuardId,
                        GuardName = n.Guard?.Username
                    }).ToList();

                    return Results.Ok(new
                    {
                        success = true,
                        count = result.Count,
                        filters = new
                        {
                            guardId,
                            fechaInicio,
                            fechaFin
                        },
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error obteniendo notas de bitácora: {ex.Message}");
                }
            })
            .WithName("GetBitacoraNotes")
            .WithSummary("Obtener notas de bitácora (todas o filtradas por guardia/fecha)");

            // Obtener notas de un guardia específico
            group.MapGet("/guardia/{guardId}", async (
                int guardId,
                IBitacoraService bitacoraService) =>
            {
                try
                {
                    var notes = await bitacoraService.GetNotesByGuardAsync(guardId);

                    var result = notes.Select(n => new
                    {
                        n.Id,
                        n.Note,
                        n.Timestamp,
                        n.GuardId,
                        GuardName = n.Guard?.Username
                    }).ToList();

                    return Results.Ok(new
                    {
                        success = true,
                        guardId,
                        count = result.Count,
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error obteniendo notas del guardia: {ex.Message}");
                }
            })
            .WithName("GetBitacoraNotesByGuard")
            .WithSummary("Obtener todas las notas de un guardia específico");

            // Buscar en bitácora
            group.MapGet("/buscar", async (
                [FromQuery] string q,
                IBitacoraService bitacoraService) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(q))
                        return Results.BadRequest("Parámetro 'q' es requerido");

                    var notes = await bitacoraService.SearchNotesAsync(q);

                    var result = notes.Select(n => new
                    {
                        n.Id,
                        n.Note,
                        n.Timestamp,
                        n.GuardId,
                        GuardName = n.Guard?.Username
                    }).ToList();

                    return Results.Ok(new
                    {
                        success = true,
                        searchTerm = q,
                        count = result.Count,
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error buscando en bitácora: {ex.Message}");
                }
            })
            .WithName("SearchBitacoraNotes")
            .WithSummary("Buscar notas en la bitácora por texto");

            // Actualizar nota (solo el que la escribió o admins)
            group.MapPut("/{id}", async (
                int id,
                [FromBody] UpdateBitacoraRequest request,
                IBitacoraService bitacoraService,
                HttpContext context) =>
            {
                try
                {
                    var currentGuardId = int.Parse(context.User.FindFirst("GuardId")?.Value ?? "1");
                    var userRole = context.User.FindFirst("Role")?.Value ?? "Guard";

                    // Verificar si la nota existe y si el usuario puede editarla
                    var existingNote = await bitacoraService.GetNoteByIdAsync(id);
                    if (existingNote == null)
                    {
                        return Results.NotFound($"Nota con ID {id} no encontrada");
                    }

                    // Solo el autor o admins pueden editar
                    if (existingNote.GuardId != currentGuardId && userRole == "Guard")
                    {
                        return Results.Forbid();
                    }

                    var note = new BitacoraNote
                    {
                        Note = request.Note,
                        GuardId = existingNote.GuardId, // Mantener el guardia original
                        Timestamp = existingNote.Timestamp // Mantener timestamp original
                    };

                    var updated = await bitacoraService.UpdateNoteAsync(id, note);

                    if (updated == null)
                    {
                        return Results.Problem("No se pudo actualizar la nota");
                    }

                    return Results.Ok(new
                    {
                        success = true,
                        message = "Nota actualizada exitosamente",
                        data = new
                        {
                            updated.Id,
                            updated.Note,
                            updated.Timestamp,
                            updated.GuardId,
                            GuardName = updated.Guard?.Username
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error actualizando nota: {ex.Message}");
                }
            })
            .WithName("UpdateBitacoraNote")
            .WithSummary("Actualizar nota de bitácora (solo autor o admins)");

            // Eliminar nota (solo admins)
            group.MapDelete("/{id}", async (
                int id,
                IBitacoraService bitacoraService,
                HttpContext context) =>
            {
                try
                {
                    var userRole = context.User.FindFirst("Role")?.Value ?? "Guard";

                    if (userRole == "Guard")
                    {
                        return Results.Forbid();
                    }

                    var success = await bitacoraService.DeleteNoteAsync(id);

                    if (!success)
                    {
                        return Results.NotFound($"Nota con ID {id} no encontrada");
                    }

                    return Results.Ok(new
                    {
                        success = true,
                        message = $"Nota {id} eliminada exitosamente"
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error eliminando nota: {ex.Message}");
                }
            })
            .WithName("DeleteBitacoraNote")
            .WithSummary("Eliminar nota de bitácora (solo admins)");
        }
    }

    public record CreateBitacoraRequest(
        string Note
    );

    public record UpdateBitacoraRequest(
        string Note
    );
}