// Routes/ApiRoutes.cs
using MicroJack.API.Routes.Modules;

namespace MicroJack.API.Routes
{
    public static class ApiRoutes
    {
        public static void Configure(WebApplication app)
        {
            // Configurar las rutas de cada módulo
            RegistrationRoutes.Configure(app);
            PreRegistrationRoutes.Configure(app);
            IntermediateRegistrationRoutes.Configure(app);
            PhidgetTestRoutes.Configure(app); // Nuevo módulo de prueba

            
            // Aquí se pueden agregar más módulos en el futuro
            // Por ejemplo:
            // UserRoutes.Configure(app);
            // ReportRoutes.Configure(app);
            // etc.
        }
    }
}