using Microsoft.EntityFrameworkCore;
using MicroJack.API.Data;
using MicroJack.API.Models.Core;
using MicroJack.API.Services.Interfaces;

namespace MicroJack.API.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(ApplicationDbContext context, ILogger<VehicleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            try
            {
                return await _context.Vehicles
                    .Include(v => v.Brand)
                    .Include(v => v.Color)
                    .Include(v => v.Type)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vehicles");
                return new List<Vehicle>();
            }
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            try
            {
                return await _context.Vehicles
                    .Include(v => v.Brand)
                    .Include(v => v.Color)
                    .Include(v => v.Type)
                    .FirstOrDefaultAsync(v => v.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle by ID {Id}", id);
                return null;
            }
        }

        public async Task<Vehicle?> GetVehicleByLicensePlateAsync(string licensePlate)
        {
            try
            {
                return await _context.Vehicles
                    .Include(v => v.Brand)
                    .Include(v => v.Color)
                    .Include(v => v.Type)
                    .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle by license plate {LicensePlate}", licensePlate);
                return null;
            }
        }

        public async Task<List<Vehicle>> SearchVehiclesAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllVehiclesAsync();
                }

                return await _context.Vehicles
                    .Include(v => v.Brand)
                    .Include(v => v.Color)
                    .Include(v => v.Type)
                    .Where(v => v.LicensePlate.Contains(searchTerm) ||
                               (v.Brand != null && v.Brand.Name.Contains(searchTerm)) ||
                               (v.Color != null && v.Color.Name.Contains(searchTerm)) ||
                               (v.Type != null && v.Type.Name.Contains(searchTerm)))
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching vehicles with term {SearchTerm}", searchTerm);
                return new List<Vehicle>();
            }
        }

        public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
        {
            try
            {
                // Check if license plate already exists
                var existingVehicle = await GetVehicleByLicensePlateAsync(vehicle.LicensePlate);
                if (existingVehicle != null)
                {
                    throw new ApplicationException($"Vehicle with license plate '{vehicle.LicensePlate}' already exists");
                }

                // Generate mock data for optional fields if not provided
                await GenerateMockDataForOptionalFieldsAsync(vehicle);

                // Validate foreign keys
                await ValidateVehicleReferencesAsync(vehicle);

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Vehicle created successfully: {LicensePlate}", vehicle.LicensePlate);
                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                throw;
            }
        }

        public async Task<Vehicle?> UpdateVehicleAsync(int id, Vehicle vehicle)
        {
            try
            {
                var existingVehicle = await _context.Vehicles.FindAsync(id);
                if (existingVehicle == null)
                {
                    return null;
                }

                // Check if new license plate conflicts with another vehicle
                if (vehicle.LicensePlate != existingVehicle.LicensePlate)
                {
                    var conflictVehicle = await GetVehicleByLicensePlateAsync(vehicle.LicensePlate);
                    if (conflictVehicle != null)
                    {
                        throw new ApplicationException($"Vehicle with license plate '{vehicle.LicensePlate}' already exists");
                    }
                }

                // Validate foreign keys
                await ValidateVehicleReferencesAsync(vehicle);

                existingVehicle.LicensePlate = vehicle.LicensePlate;
                existingVehicle.PlateImageUrl = vehicle.PlateImageUrl;
                existingVehicle.BrandId = vehicle.BrandId;
                existingVehicle.ColorId = vehicle.ColorId;
                existingVehicle.TypeId = vehicle.TypeId;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Vehicle updated successfully: {Id}", id);
                return existingVehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    return false;
                }

                // Check if vehicle has associated access logs
                var hasAccessLogs = await _context.AccessLogs
                    .AnyAsync(al => al.VehicleId == id);

                if (hasAccessLogs)
                {
                    throw new ApplicationException("Cannot delete vehicle with associated access logs");
                }

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Vehicle deleted successfully: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {Id}", id);
                throw;
            }
        }

        public async Task<List<Vehicle>> GetVehiclesByBrandAsync(int brandId)
        {
            try
            {
                return await _context.Vehicles
                    .Include(v => v.Brand)
                    .Include(v => v.Color)
                    .Include(v => v.Type)
                    .Where(v => v.BrandId == brandId)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles by brand {BrandId}", brandId);
                return new List<Vehicle>();
            }
        }

        public async Task<List<Vehicle>> GetVehiclesByColorAsync(int colorId)
        {
            try
            {
                return await _context.Vehicles
                    .Include(v => v.Brand)
                    .Include(v => v.Color)
                    .Include(v => v.Type)
                    .Where(v => v.ColorId == colorId)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles by color {ColorId}", colorId);
                return new List<Vehicle>();
            }
        }

        public async Task<List<Vehicle>> GetVehiclesByTypeAsync(int typeId)
        {
            try
            {
                return await _context.Vehicles
                    .Include(v => v.Brand)
                    .Include(v => v.Color)
                    .Include(v => v.Type)
                    .Where(v => v.TypeId == typeId)
                    .OrderBy(v => v.LicensePlate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles by type {TypeId}", typeId);
                return new List<Vehicle>();
            }
        }

        private async Task ValidateVehicleReferencesAsync(Vehicle vehicle)
        {
            if (vehicle.BrandId.HasValue)
            {
                var brandExists = await _context.VehicleBrands.AnyAsync(b => b.Id == vehicle.BrandId);
                if (!brandExists)
                {
                    throw new ApplicationException($"Vehicle brand with ID {vehicle.BrandId} does not exist");
                }
            }

            if (vehicle.ColorId.HasValue)
            {
                var colorExists = await _context.VehicleColors.AnyAsync(c => c.Id == vehicle.ColorId);
                if (!colorExists)
                {
                    throw new ApplicationException($"Vehicle color with ID {vehicle.ColorId} does not exist");
                }
            }

            if (vehicle.TypeId.HasValue)
            {
                var typeExists = await _context.VehicleTypes.AnyAsync(t => t.Id == vehicle.TypeId);
                if (!typeExists)
                {
                    throw new ApplicationException($"Vehicle type with ID {vehicle.TypeId} does not exist");
                }
            }
        }

        private async Task GenerateMockDataForOptionalFieldsAsync(Vehicle vehicle)
        {
            // If no plate image URL is provided, set a default placeholder
            if (string.IsNullOrEmpty(vehicle.PlateImageUrl))
            {
                vehicle.PlateImageUrl = $"/uploads/plates/{vehicle.LicensePlate.ToLower()}_placeholder.jpg";
            }

            // If no brand is specified, get a random brand or use the first available
            if (!vehicle.BrandId.HasValue)
            {
                var brands = await _context.VehicleBrands.ToListAsync();
                if (brands.Any())
                {
                    var random = new Random();
                    vehicle.BrandId = brands[random.Next(brands.Count)].Id;
                }
            }

            // If no color is specified, get a random color or use the first available
            if (!vehicle.ColorId.HasValue)
            {
                var colors = await _context.VehicleColors.ToListAsync();
                if (colors.Any())
                {
                    var random = new Random();
                    vehicle.ColorId = colors[random.Next(colors.Count)].Id;
                }
            }

            // If no type is specified, get a random type or use the first available
            if (!vehicle.TypeId.HasValue)
            {
                var types = await _context.VehicleTypes.ToListAsync();
                if (types.Any())
                {
                    var random = new Random();
                    vehicle.TypeId = types[random.Next(types.Count)].Id;
                }
            }
        }
    }
}