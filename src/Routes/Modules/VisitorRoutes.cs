using MicroJack.API.Services.Interfaces;
using MicroJack.API.Models.Core;

namespace MicroJack.API.Routes.Modules
{
    public static class VisitorRoutes
    {
        public static void MapVisitorRoutes(this WebApplication app)
        {
            var visitorGroup = app.MapGroup("/api/visitors").WithTags("Visitors");

            // GET all visitors
            visitorGroup.MapGet("/", async (IVisitorService visitorService) =>
            {
                try
                {
                    var visitors = await visitorService.GetAllVisitorsAsync();
                    return Results.Ok(new { success = true, data = visitors });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting visitors", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetAllVisitors")
            .Produces<object>(200);

            // GET visitor by ID
            visitorGroup.MapGet("/{id:int}", async (int id, IVisitorService visitorService) =>
            {
                try
                {
                    var visitor = await visitorService.GetVisitorByIdAsync(id);
                    if (visitor == null)
                        return Results.NotFound(new { success = false, message = "Visitor not found" });

                    return Results.Ok(new { success = true, data = visitor });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error getting visitor", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("GetVisitorById")
            .Produces<object>(200)
            .Produces(404);

            // POST create new visitor
            visitorGroup.MapPost("/", async (VisitorCreateRequest request, IVisitorService visitorService) =>
            {
                try
                {
                    var visitor = new Visitor
                    {
                        FullName = request.FullName,
                        IneImageUrl = request.IneImageUrl,
                        FaceImageUrl = request.FaceImageUrl
                    };

                    var createdVisitor = await visitorService.CreateVisitorAsync(visitor);
                    return Results.Created($"/api/visitors/{createdVisitor.Id}", new { success = true, data = createdVisitor });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error creating visitor", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("CreateVisitor")
            .Produces<object>(201)
            .Produces(500);

            // PUT update visitor
            visitorGroup.MapPut("/{id:int}", async (int id, VisitorUpdateRequest request, IVisitorService visitorService) =>
            {
                try
                {
                    var visitor = new Visitor
                    {
                        FullName = request.FullName,
                        IneImageUrl = request.IneImageUrl,
                        FaceImageUrl = request.FaceImageUrl
                    };

                    var updatedVisitor = await visitorService.UpdateVisitorAsync(id, visitor);
                    if (updatedVisitor == null)
                        return Results.NotFound(new { success = false, message = "Visitor not found" });

                    return Results.Ok(new { success = true, data = updatedVisitor });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error updating visitor", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("UpdateVisitor")
            .Produces<object>(200)
            .Produces(404);

            // DELETE visitor
            visitorGroup.MapDelete("/{id:int}", async (int id, IVisitorService visitorService) =>
            {
                try
                {
                    var result = await visitorService.DeleteVisitorAsync(id);
                    if (!result)
                        return Results.NotFound(new { success = false, message = "Visitor not found" });

                    return Results.Ok(new { success = true, message = "Visitor deleted successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "Error deleting visitor", detail: ex.Message, statusCode: 500);
                }
            })
            .RequireAuthorization()
            .WithName("DeleteVisitor")
            .Produces<object>(200)
            .Produces(404);
        }
    }

    // DTOs
    public class VisitorCreateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? IneImageUrl { get; set; }
        public string? FaceImageUrl { get; set; }
    }

    public class VisitorUpdateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? IneImageUrl { get; set; }
        public string? FaceImageUrl { get; set; }
    }
}