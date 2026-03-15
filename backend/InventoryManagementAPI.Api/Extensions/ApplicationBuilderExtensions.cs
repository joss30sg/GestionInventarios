using Microsoft.EntityFrameworkCore;
using InventoryManagementAPI.Api.Middleware;
using InventoryManagementAPI.Infrastructure.Hubs;
using InventoryManagementAPI.Infrastructure.Data;
using Serilog;

namespace InventoryManagementAPI.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UseApplicationPipeline(this WebApplication app)
    {
        // Habilitar Swagger en desarrollo con configuración profesional
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "api-docs/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/api-docs/v1/swagger.json", "📦 Inventory Management API v1.0.0");
                
                // Configuración visual y de UX
                options.RoutePrefix = string.Empty;  // Documentación en raíz (localhost:5001)
                options.DocumentTitle = "📦 Inventory Management API - Swagger Documentation";
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
                options.EnableTryItOutByDefault();
                options.ShowExtensions();
                
                // Configuración avanzada
                options.ConfigObject.AdditionalItems["persistAuthorization"] = true;
                options.ConfigObject.DefaultModelsExpandDepth = 1;
                options.ConfigObject.DefaultModelExpandDepth = 2;
                options.ConfigObject.DocExpansion = Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List;
            });
        }

        // CORS - DEBE ir antes de UseHttpsRedirection
        app.UseCors("AllowReactApp");

        // HTTPS redirect (solo en producción)
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // Request logging
        app.UseRequestLogging();

        // Global exception handling
        app.UseGlobalExceptionHandler();

        // Authentication and Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Routes
        app.MapControllers();
        app.MapHealthChecks("/health");
        
        // SignalR Hubs for real-time notifications
        app.MapHub<InventoryNotificationHub>("/hubs/inventory-notifications");
        Log.Information("[SIGNALR] Hub de notificaciones disponible en /hubs/inventory-notifications");
    }

    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            Log.Information("[MIGRACIONES] Aplicando migraciones de base de datos...");
            await dbContext.Database.MigrateAsync();
            Log.Information("[OK] Migraciones completadas");

            await DatabaseSeeder.SeedAsync(dbContext, logger);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ERROR] Migraciones fallidas: {Message}", ex.Message);
            throw;
        }
    }
}
