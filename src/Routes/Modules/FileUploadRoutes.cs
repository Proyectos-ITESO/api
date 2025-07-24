using System.ComponentModel.DataAnnotations;

namespace MicroJack.API.Routes.Modules
{
    public static class FileUploadRoutes
    {
        public static void MapFileUploadRoutes(this WebApplication app)
        {
            var uploadGroup = app.MapGroup("/api/upload").WithTags("File Upload");

            // POST upload image
            uploadGroup.MapPost("/image", async (IFormFile file, string? category, IWebHostEnvironment environment) =>
            {
                try
                {
                    // Validate file
                    if (file == null || file.Length == 0)
                    {
                        return Results.BadRequest(new { 
                            success = false, 
                            message = "No file uploaded" 
                        });
                    }

                    // Validate file size (max 5MB)
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        return Results.BadRequest(new { 
                            success = false, 
                            message = "File size cannot exceed 5MB" 
                        });
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return Results.BadRequest(new { 
                            success = false, 
                            message = "Only image files are allowed (jpg, jpeg, png, gif, bmp)" 
                        });
                    }

                    // Validate MIME type
                    var allowedMimeTypes = new[] { 
                        "image/jpeg", "image/jpg", "image/png", 
                        "image/gif", "image/bmp" 
                    };
                    
                    if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                    {
                        return Results.BadRequest(new { 
                            success = false, 
                            message = "Invalid file type" 
                        });
                    }

                    // Create uploads directory structure
                    var uploadsPath = Path.Combine(environment.ContentRootPath, "uploads");
                    var categoryPath = string.IsNullOrEmpty(category) ? "general" : category.ToLowerInvariant();
                    var fullUploadPath = Path.Combine(uploadsPath, categoryPath);
                    
                    Directory.CreateDirectory(fullUploadPath);

                    // Generate unique filename
                    var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                    var uniqueId = Guid.NewGuid().ToString("N")[..8];
                    var fileName = $"{timestamp}_{uniqueId}{fileExtension}";
                    var filePath = Path.Combine(fullUploadPath, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Generate URL for accessing the file
                    var fileUrl = $"/uploads/{categoryPath}/{fileName}";

                    return Results.Ok(new
                    {
                        success = true,
                        message = "File uploaded successfully",
                        data = new
                        {
                            fileName = file.FileName,
                            savedFileName = fileName,
                            url = fileUrl,
                            category = categoryPath,
                            size = file.Length,
                            contentType = file.ContentType,
                            uploadedAt = DateTime.UtcNow
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Error uploading file",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("UploadImage")
            .WithSummary("Upload an image file (face, INE, license plate, etc.)")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<object>(200)
            .Produces(400)
            .Produces(500);

            // POST upload multiple images
            uploadGroup.MapPost("/images", async (IFormFileCollection files, string? category, IWebHostEnvironment environment) =>
            {
                try
                {
                    if (files == null || files.Count == 0)
                    {
                        return Results.BadRequest(new { 
                            success = false, 
                            message = "No files uploaded" 
                        });
                    }

                    if (files.Count > 10)
                    {
                        return Results.BadRequest(new { 
                            success = false, 
                            message = "Cannot upload more than 10 files at once" 
                        });
                    }

                    var results = new List<object>();
                    var errors = new List<string>();

                    foreach (var file in files)
                    {
                        try
                        {
                            // Same validation as single file upload
                            if (file.Length > 5 * 1024 * 1024)
                            {
                                errors.Add($"{file.FileName}: File size exceeds 5MB");
                                continue;
                            }

                            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                            
                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                errors.Add($"{file.FileName}: Invalid file type");
                                continue;
                            }

                            var allowedMimeTypes = new[] { 
                                "image/jpeg", "image/jpg", "image/png", 
                                "image/gif", "image/bmp" 
                            };
                            
                            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                            {
                                errors.Add($"{file.FileName}: Invalid MIME type");
                                continue;
                            }

                            // Create uploads directory
                            var uploadsPath = Path.Combine(environment.ContentRootPath, "uploads");
                            var categoryPath = string.IsNullOrEmpty(category) ? "general" : category.ToLowerInvariant();
                            var fullUploadPath = Path.Combine(uploadsPath, categoryPath);
                            
                            Directory.CreateDirectory(fullUploadPath);

                            // Generate unique filename
                            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                            var uniqueId = Guid.NewGuid().ToString("N")[..8];
                            var fileName = $"{timestamp}_{uniqueId}{fileExtension}";
                            var filePath = Path.Combine(fullUploadPath, fileName);

                            // Save file
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            // Generate URL
                            var fileUrl = $"/uploads/{categoryPath}/{fileName}";

                            results.Add(new
                            {
                                fileName = file.FileName,
                                savedFileName = fileName,
                                url = fileUrl,
                                size = file.Length,
                                contentType = file.ContentType
                            });
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{file.FileName}: {ex.Message}");
                        }
                    }

                    return Results.Ok(new
                    {
                        success = true,
                        message = $"Uploaded {results.Count} files successfully",
                        data = new
                        {
                            uploaded = results,
                            errors = errors,
                            category = category ?? "general",
                            uploadedAt = DateTime.UtcNow
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Error uploading files",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .RequireAuthorization("GuardLevel")
            .WithName("UploadMultipleImages")
            .WithSummary("Upload multiple image files at once")
            .Accepts<IFormFileCollection>("multipart/form-data")
            .Produces<object>(200)
            .Produces(400)
            .Produces(500);

            // DELETE uploaded file
            uploadGroup.MapDelete("/image", async (string fileUrl, IWebHostEnvironment environment) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(fileUrl))
                    {
                        return Results.BadRequest(new { 
                            success = false, 
                            message = "File URL is required" 
                        });
                    }

                    // Extract file path from URL
                    var urlPath = fileUrl.TrimStart('/');
                    var filePath = Path.Combine(environment.ContentRootPath, urlPath);

                    // Security check: ensure file is within uploads directory
                    var uploadsPath = Path.Combine(environment.ContentRootPath, "uploads");
                    var fullFilePath = Path.GetFullPath(filePath);
                    var fullUploadsPath = Path.GetFullPath(uploadsPath);

                    if (!fullFilePath.StartsWith(fullUploadsPath))
                    {
                        return Results.BadRequest(new { 
                            success = false, 
                            message = "Invalid file path" 
                        });
                    }

                    if (!File.Exists(filePath))
                    {
                        return Results.NotFound(new { 
                            success = false, 
                            message = "File not found" 
                        });
                    }

                    File.Delete(filePath);

                    return Results.Ok(new
                    {
                        success = true,
                        message = "File deleted successfully",
                        deletedUrl = fileUrl
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Error deleting file",
                        detail: ex.Message,
                        statusCode: 500
                    );
                }
            })
            .RequireAuthorization("AdminLevel")
            .WithName("DeleteUploadedFile")
            .WithSummary("Delete an uploaded file")
            .Produces<object>(200)
            .Produces(400)
            .Produces(404)
            .Produces(500);
        }
    }
}