namespace MicroJack.API.Models.Enums
{
    public enum Permission
    {
        // Access Log permissions
        ViewAccessLogs = 1,
        CreateAccessLog = 2,
        UpdateAccessLog = 3,
        DeleteAccessLog = 4,
        RegisterExit = 5,

        // Guard management permissions
        ViewGuards = 10,
        CreateGuard = 11,
        UpdateGuard = 12,
        DeleteGuard = 13,
        ManageRoles = 14,

        // Visitor management
        ViewVisitors = 20,
        CreateVisitor = 21,
        UpdateVisitor = 22,
        DeleteVisitor = 23,

        // Vehicle management
        ViewVehicles = 30,
        CreateVehicle = 31,
        UpdateVehicle = 32,
        DeleteVehicle = 33,

        // Address management
        ViewAddresses = 40,
        CreateAddress = 41,
        UpdateAddress = 42,
        DeleteAddress = 43,

        // Catalog management
        ViewCatalogs = 50,
        ManageCatalogs = 51,

        // Reports
        ViewReports = 60,
        ExportReports = 61,

        // System administration
        ViewEventLogs = 70,
        ManageSystem = 71,
        ViewDashboard = 72,

        // Super admin permissions
        SuperAdmin = 999
    }

    public static class PermissionExtensions
    {
        public static string GetDescription(this Permission permission)
        {
            return permission switch
            {
                Permission.ViewAccessLogs => "Ver registros de acceso",
                Permission.CreateAccessLog => "Crear registros de acceso",
                Permission.UpdateAccessLog => "Actualizar registros de acceso",
                Permission.DeleteAccessLog => "Eliminar registros de acceso",
                Permission.RegisterExit => "Registrar salidas",
                
                Permission.ViewGuards => "Ver guardias",
                Permission.CreateGuard => "Crear guardias",
                Permission.UpdateGuard => "Actualizar guardias",
                Permission.DeleteGuard => "Eliminar guardias",
                Permission.ManageRoles => "Gestionar roles",
                
                Permission.ViewVisitors => "Ver visitantes",
                Permission.CreateVisitor => "Crear visitantes",
                Permission.UpdateVisitor => "Actualizar visitantes",
                Permission.DeleteVisitor => "Eliminar visitantes",
                
                Permission.ViewVehicles => "Ver vehículos",
                Permission.CreateVehicle => "Crear vehículos",
                Permission.UpdateVehicle => "Actualizar vehículos",
                Permission.DeleteVehicle => "Eliminar vehículos",
                
                Permission.ViewAddresses => "Ver direcciones",
                Permission.CreateAddress => "Crear direcciones",
                Permission.UpdateAddress => "Actualizar direcciones",
                Permission.DeleteAddress => "Eliminar direcciones",
                
                Permission.ViewCatalogs => "Ver catálogos",
                Permission.ManageCatalogs => "Gestionar catálogos",
                
                Permission.ViewReports => "Ver reportes",
                Permission.ExportReports => "Exportar reportes",
                
                Permission.ViewEventLogs => "Ver bitácora de eventos",
                Permission.ManageSystem => "Administrar sistema",
                Permission.ViewDashboard => "Ver dashboard",
                
                Permission.SuperAdmin => "Super Administrador",
                
                _ => permission.ToString()
            };
        }
    }
}