using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Models.Catalog;

namespace MicroJack.API.Services
{
    public class DatabaseInitializationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseInitializationService> _logger;

        public DatabaseInitializationService(ApplicationDbContext context, ILogger<DatabaseInitializationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("Database created/verified successfully");

                // Seed catalog data
                await SeedCatalogDataAsync();
                
                // Seed roles and initial admin
                await SeedRolesAsync();
                await SeedInitialAdminAsync();

                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database initialization");
                throw;
            }
        }

        private async Task SeedCatalogDataAsync()
        {
            // Seed Vehicle Brands
            if (!await _context.VehicleBrands.AnyAsync())
            {
                var brands = new[]
                {
                    "Nissan", "Toyota", "Chevrolet", "Ford", "Honda", "Mazda", "Hyundai", "Kia",
                    "Volkswagen", "BMW", "Mercedes-Benz", "Audi", "Peugeot", "Renault", "Jeep",
                    "Dodge", "Chrysler", "Mitsubishi", "Subaru", "Suzuki", "Isuzu", "JAC",
                    "SEAT", "Fiat", "Alfa Romeo", "Volvo", "Infiniti", "Lexus", "Acura",
                    "Cadillac", "Lincoln", "Buick", "GMC", "Pontiac", "Otro"
                };

                foreach (var brand in brands)
                {
                    _context.VehicleBrands.Add(new VehicleBrand { Name = brand });
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Vehicle brands seeded successfully");
            }

            // Seed Vehicle Colors
            if (!await _context.VehicleColors.AnyAsync())
            {
                var colors = new[]
                {
                    "Blanco", "Negro", "Gris", "Plata", "Azul", "Rojo", "Verde", "Amarillo",
                    "Naranja", "Café", "Beige", "Dorado", "Morado", "Rosa", "Turquesa",
                    "Azul Marino", "Gris Oscuro", "Gris Claro", "Rojo Vino", "Verde Militar",
                    "Azul Rey", "Otro"
                };

                foreach (var color in colors)
                {
                    _context.VehicleColors.Add(new VehicleColor { Name = color });
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Vehicle colors seeded successfully");
            }

            // Seed Vehicle Types
            if (!await _context.VehicleTypes.AnyAsync())
            {
                var types = new[]
                {
                    "Automóvil", "Camioneta", "SUV", "Motocicleta", "Bicicleta", "Camión",
                    "Van", "Pick-up", "Deportivo", "Convertible", "Hatchback", "Sedán",
                    "Coupé", "Peatón"
                };

                foreach (var type in types)
                {
                    _context.VehicleTypes.Add(new VehicleType { Name = type });
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Vehicle types seeded successfully");
            }

            // Seed Visit Reasons
            if (!await _context.VisitReasons.AnyAsync())
            {
                var reasons = new[]
                {
                    "Visitante", "Proveedor", "Paquetería", "Servicio doméstico", "Reparaciones",
                    "Delivery", "Médico", "Familiar", "Amigo", "Trabajo", "Mantenimiento",
                    "Jardinería", "Limpieza", "Seguridad", "Emergencia", "Otro"
                };

                foreach (var reason in reasons)
                {
                    _context.VisitReasons.Add(new VisitReason { Reason = reason });
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Visit reasons seeded successfully");
            }
        }

        private async Task SeedRolesAsync()
        {
            if (!await _context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Role
                    {
                        Name = "SuperAdmin",
                        Description = "Super Administrador con todos los permisos",
                        Permissions = System.Text.Json.JsonSerializer.Serialize(new[] { MicroJack.API.Models.Enums.Permission.SuperAdmin })
                    },
                    new Role
                    {
                        Name = "Admin",
                        Description = "Administrador con permisos de gestión",
                        Permissions = System.Text.Json.JsonSerializer.Serialize(new[]
                        {
                            MicroJack.API.Models.Enums.Permission.ViewAccessLogs,
                            MicroJack.API.Models.Enums.Permission.CreateAccessLog,
                            MicroJack.API.Models.Enums.Permission.UpdateAccessLog,
                            MicroJack.API.Models.Enums.Permission.RegisterExit,
                            MicroJack.API.Models.Enums.Permission.ViewGuards,
                            MicroJack.API.Models.Enums.Permission.CreateGuard,
                            MicroJack.API.Models.Enums.Permission.UpdateGuard,
                            MicroJack.API.Models.Enums.Permission.ViewVisitors,
                            MicroJack.API.Models.Enums.Permission.CreateVisitor,
                            MicroJack.API.Models.Enums.Permission.UpdateVisitor,
                            MicroJack.API.Models.Enums.Permission.ViewVehicles,
                            MicroJack.API.Models.Enums.Permission.CreateVehicle,
                            MicroJack.API.Models.Enums.Permission.UpdateVehicle,
                            MicroJack.API.Models.Enums.Permission.ViewAddresses,
                            MicroJack.API.Models.Enums.Permission.ViewCatalogs,
                            MicroJack.API.Models.Enums.Permission.ManageCatalogs,
                            MicroJack.API.Models.Enums.Permission.ViewReports,
                            MicroJack.API.Models.Enums.Permission.ViewEventLogs,
                            MicroJack.API.Models.Enums.Permission.ViewDashboard
                        })
                    },
                    new Role
                    {
                        Name = "Guard",
                        Description = "Guardia con permisos básicos de operación",
                        Permissions = System.Text.Json.JsonSerializer.Serialize(new[]
                        {
                            MicroJack.API.Models.Enums.Permission.ViewAccessLogs,
                            MicroJack.API.Models.Enums.Permission.CreateAccessLog,
                            MicroJack.API.Models.Enums.Permission.RegisterExit,
                            MicroJack.API.Models.Enums.Permission.ViewVisitors,
                            MicroJack.API.Models.Enums.Permission.CreateVisitor,
                            MicroJack.API.Models.Enums.Permission.ViewVehicles,
                            MicroJack.API.Models.Enums.Permission.CreateVehicle,
                            MicroJack.API.Models.Enums.Permission.ViewAddresses,
                            MicroJack.API.Models.Enums.Permission.ViewCatalogs
                        })
                    }
                };

                foreach (var role in roles)
                {
                    _context.Roles.Add(role);
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Default roles seeded successfully");
            }
        }

        private async Task SeedInitialAdminAsync()
        {
            if (!await _context.Guards.AnyAsync())
            {
                // Create default admin guard
                var defaultAdmin = new Guard
                {
                    FullName = "Super Administrador",
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Change this in production
                    IsActive = true
                };

                _context.Guards.Add(defaultAdmin);
                await _context.SaveChangesAsync();

                // Assign SuperAdmin role
                var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
                if (superAdminRole != null)
                {
                    var guardRole = new GuardRole
                    {
                        GuardId = defaultAdmin.Id,
                        RoleId = superAdminRole.Id,
                        AssignedBy = defaultAdmin.Id // Self-assigned
                    };
                    
                    _context.GuardRoles.Add(guardRole);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Initial admin created successfully");
            }
        }
    }
}