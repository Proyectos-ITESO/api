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

                // Seed test data
                await SeedTestDataAsync();

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
                    "Jardinería", "Limpieza", "Seguridad", "Emergencia", "Test", "Otro"
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

        private async Task SeedTestDataAsync()
        {
            // Seed test addresses first
            if (!await _context.Addresses.AnyAsync())
            {
                var testAddresses = new[]
                {
                    new Address
                    {
                        Identifier = "102",
                        Extension = "102-1",
                        Status = "Activa"
                    },
                    new Address
                    {
                        Identifier = "5",
                        Extension = "5-A",
                        Status = "Activa"
                    },
                    new Address
                    {
                        Identifier = "207",
                        Extension = "207-2",
                        Status = "Activa"
                    },
                    new Address
                    {
                        Identifier = "15",
                        Extension = "15-B",
                        Status = "Activa"
                    },
                    new Address
                    {
                        Identifier = "301",
                        Extension = "301-3",
                        Status = "Activa"
                    },
                    new Address
                    {
                        Identifier = "42",
                        Extension = "42-C",
                        Status = "Activa"
                    },
                    new Address
                    {
                        Identifier = "156",
                        Extension = "156-1",
                        Status = "Activa"
                    },
                    new Address
                    {
                        Identifier = "89",
                        Extension = "89-D",
                        Status = "Activa"
                    },
                    new Address
                    {
                        Identifier = "234",
                        Extension = "234-2",
                        Status = "Activa"
                    },
                    new Address
                    {
                        Identifier = "67",
                        Extension = "67-A",
                        Status = "Activa"
                    }
                };

                foreach (var address in testAddresses)
                {
                    _context.Addresses.Add(address);
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Test addresses seeded successfully");
            }

            // Seed test residents
            if (!await _context.Residents.AnyAsync())
            {
                var addresses = await _context.Addresses.ToListAsync();
                
                var testResidents = new[]
                {
                    new Resident
                    {
                        FullName = "Juan Pérez García",
                        Phone = "3331234567",
                        AddressId = addresses.First(a => a.Identifier == "102" && a.Extension == "102-1").Id
                    },
                    new Resident
                    {
                        FullName = "María López Hernández",
                        Phone = "3331234568",
                        AddressId = addresses.First(a => a.Identifier == "5" && a.Extension == "5-A").Id
                    },
                    new Resident
                    {
                        FullName = "Carlos Ramírez Sánchez",
                        Phone = "3331234569",
                        AddressId = addresses.First(a => a.Identifier == "207" && a.Extension == "207-2").Id
                    },
                    new Resident
                    {
                        FullName = "Ana Martínez Torres",
                        Phone = "3331234570",
                        AddressId = addresses.First(a => a.Identifier == "15" && a.Extension == "15-B").Id
                    },
                    new Resident
                    {
                        FullName = "Pedro González Villa",
                        Phone = "3331234571",
                        AddressId = addresses.First(a => a.Identifier == "301" && a.Extension == "301-3").Id
                    },
                    new Resident
                    {
                        FullName = "Laura Jiménez Cruz",
                        Phone = "3331234572",
                        AddressId = addresses.First(a => a.Identifier == "42" && a.Extension == "42-C").Id
                    },
                    new Resident
                    {
                        FullName = "Roberto Silva Morales",
                        Phone = "3331234573",
                        AddressId = addresses.First(a => a.Identifier == "156" && a.Extension == "156-1").Id
                    },
                    new Resident
                    {
                        FullName = "Carmen Ruiz Flores",
                        Phone = "3331234574",
                        AddressId = addresses.First(a => a.Identifier == "89" && a.Extension == "89-D").Id
                    },
                    new Resident
                    {
                        FullName = "Fernando Castro Vega",
                        Phone = "3331234575",
                        AddressId = addresses.First(a => a.Identifier == "234" && a.Extension == "234-2").Id
                    },
                    new Resident
                    {
                        FullName = "Diana Moreno Aguilar",
                        Phone = "3331234576",
                        AddressId = addresses.First(a => a.Identifier == "67" && a.Extension == "67-A").Id
                    }
                };

                foreach (var resident in testResidents)
                {
                    _context.Residents.Add(resident);
                }
                await _context.SaveChangesAsync();

                // Update addresses with representative residents
                foreach (var address in addresses)
                {
                    var firstResident = await _context.Residents.FirstOrDefaultAsync(r => r.AddressId == address.Id);
                    if (firstResident != null)
                    {
                        address.RepresentativeResidentId = firstResident.Id;
                    }
                }
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Test residents seeded successfully");
            }

            // Seed test vehicles
            if (!await _context.Vehicles.AnyAsync())
            {
                var nissan = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.Name == "Nissan");
                var toyota = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.Name == "Toyota");
                var chevrolet = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.Name == "Chevrolet");
                var ford = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.Name == "Ford");
                var honda = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.Name == "Honda");

                var blanco = await _context.VehicleColors.FirstOrDefaultAsync(c => c.Name == "Blanco");
                var negro = await _context.VehicleColors.FirstOrDefaultAsync(c => c.Name == "Negro");
                var gris = await _context.VehicleColors.FirstOrDefaultAsync(c => c.Name == "Gris");
                var azul = await _context.VehicleColors.FirstOrDefaultAsync(c => c.Name == "Azul");
                var rojo = await _context.VehicleColors.FirstOrDefaultAsync(c => c.Name == "Rojo");

                var automovil = await _context.VehicleTypes.FirstOrDefaultAsync(t => t.Name == "Automóvil");
                var suv = await _context.VehicleTypes.FirstOrDefaultAsync(t => t.Name == "SUV");
                var pickup = await _context.VehicleTypes.FirstOrDefaultAsync(t => t.Name == "Pick-up");
                var camioneta = await _context.VehicleTypes.FirstOrDefaultAsync(t => t.Name == "Camioneta");

                var testVehicles = new[]
                {
                    new Vehicle
                    {
                        LicensePlate = "ABC-123",
                        BrandId = nissan?.Id,
                        ColorId = blanco?.Id,
                        TypeId = automovil?.Id
                    },
                    new Vehicle
                    {
                        LicensePlate = "DEF-456",
                        BrandId = toyota?.Id,
                        ColorId = negro?.Id,
                        TypeId = suv?.Id
                    },
                    new Vehicle
                    {
                        LicensePlate = "GHI-789",
                        BrandId = chevrolet?.Id,
                        ColorId = gris?.Id,
                        TypeId = pickup?.Id
                    },
                    new Vehicle
                    {
                        LicensePlate = "JKL-012",
                        BrandId = ford?.Id,
                        ColorId = azul?.Id,
                        TypeId = automovil?.Id
                    },
                    new Vehicle
                    {
                        LicensePlate = "MNO-345",
                        BrandId = honda?.Id,
                        ColorId = rojo?.Id,
                        TypeId = camioneta?.Id
                    },
                    new Vehicle
                    {
                        LicensePlate = "PQR-678",
                        BrandId = nissan?.Id,
                        ColorId = gris?.Id,
                        TypeId = suv?.Id
                    },
                    new Vehicle
                    {
                        LicensePlate = "STU-901",
                        BrandId = toyota?.Id,
                        ColorId = blanco?.Id,
                        TypeId = automovil?.Id
                    },
                    new Vehicle
                    {
                        LicensePlate = "VWX-234",
                        BrandId = ford?.Id,
                        ColorId = negro?.Id,
                        TypeId = pickup?.Id
                    }
                };

                foreach (var vehicle in testVehicles)
                {
                    _context.Vehicles.Add(vehicle);
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Test vehicles seeded successfully");
            }
        }
    }
}