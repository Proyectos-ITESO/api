// Routes/ApiRoutes.cs
using MicroJack.API.Routes.Modules;

namespace MicroJack.API.Routes
{
    public static class ApiRoutes
    {
        public static void Configure(WebApplication app)
        {
            // Configure authentication routes (no auth required)
            app.MapAuthRoutes();
            
            // Configure new normalized database routes
            app.MapGuardRoutes();
            app.MapAddressRoutes();
            app.MapVehicleRoutes();
            app.MapVisitorRoutes();
            app.MapResidentRoutes();
            app.MapAccessLogRoutes();
            app.MapCatalogRoutes();
            
            // Configurar las rutas de cada módulo legacy
            RegistrationRoutes.Configure(app);
            PreRegistrationRoutes.Configure(app);
            IntermediateRegistrationRoutes.Configure(app);
            PhidgetTestRoutes.Configure(app); // Nuevo módulo de prueba

            
            // Aquí se pueden agregar más módulos en el futuro
            // Por ejemplo:
            // VisitorRoutes.Configure(app);
            // AccessLogRoutes.Configure(app);
            // etc.
        }
    }
}