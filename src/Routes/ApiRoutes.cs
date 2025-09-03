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

            // Configure enhanced routes for frontend integration
            app.MapUnifiedAccessRoutes();
            app.MapFileUploadRoutes();
            app.MapEventLogRoutes();
            app.MapSimplePreRegistrationRoutes();
            app.MapResidentManagementRoutes();
            app.MapBitacoraRoutes();
            app.MapHousesRoutes();
            app.MapUpdateRoutes();
            app.MapTelephonyRoutes();

            // Configure administration routes
            app.MapAdminRoutes();

            // PhidgetTestRoutes.Configure(app); // Hardware testing module
        }
    }
}
