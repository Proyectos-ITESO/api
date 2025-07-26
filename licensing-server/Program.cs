using Microsoft.AspNetCore.Mvc;
using LicensingServer.Models;
using LicensingServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<LicenseValidationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/validate", ([FromQuery] string licenseKey, [FromQuery] string machineId, LicenseValidationService service) =>
{
    var response = service.ValidateAndSign(licenseKey, machineId);
    return response != null ? Results.Ok(response) : Results.NotFound("Invalid license or machine ID.");
    })
.WithName("ValidateLicense")
.WithOpenApi();
 
app.Run();
