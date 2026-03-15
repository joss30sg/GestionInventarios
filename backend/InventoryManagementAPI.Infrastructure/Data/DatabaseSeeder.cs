using InventoryManagementAPI.Domain.Entities;
using InventoryManagementAPI.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementAPI.Infrastructure.Data;

/// <summary>
/// Clase de semilla de datos para llenar la base de datos.
/// Crea solo los usuarios: Admin y Empleado
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Siembra datos iniciales en la base de datos.
    /// Crea los usuarios: admin y empleado
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            // Limpiar usuarios existentes
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("[BD] Limpiando usuarios existentes...");
                var existingUsers = context.Users.ToList();
                context.Users.RemoveRange(existingUsers);
                await context.SaveChangesAsync();
            }

            logger.LogInformation("[SEEDING] Creando usuarios del sistema...");

            // Crear usuarios
            var users = new List<User>
            {
                // Administrador del sistema
                new User
                {
                    Username = "admin",
                    Email = "admin@inventory.local",
                    PasswordHash = PasswordHasher.Hash("Admin123!"),
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Usuario Empleado
                new User
                {
                    Username = "empleado",
                    Email = "empleado@inventory.local",
                    PasswordHash = PasswordHasher.Hash("Empleado123!"),
                    Role = UserRole.Employee,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            logger.LogInformation("[OK] Usuarios creados correctamente");

            // Crear productos de ejemplo
            if (!await context.Products.AnyAsync())
            {
                logger.LogInformation("[SEEDING] Creando productos de ejemplo...");

                var products = new List<Product>
                {
                    new Product { Name = "Laptop HP ProBook", Description = "Laptop empresarial 14 pulgadas, 16GB RAM, 512GB SSD", Price = 3499.99m, Quantity = 25, Category = "Electrónica", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Monitor Samsung 27\"", Description = "Monitor LED Full HD 27 pulgadas", Price = 899.50m, Quantity = 40, Category = "Electrónica", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Teclado Mecánico Logitech", Description = "Teclado mecánico RGB switches brown", Price = 349.90m, Quantity = 60, Category = "Periféricos", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Mouse Inalámbrico", Description = "Mouse ergonómico inalámbrico 2.4GHz", Price = 89.90m, Quantity = 100, Category = "Periféricos", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Silla Ergonómica", Description = "Silla de oficina con soporte lumbar ajustable", Price = 1299.00m, Quantity = 15, Category = "Mobiliario", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Escritorio Ajustable", Description = "Escritorio de pie/sentado motorizado 160x80cm", Price = 2199.00m, Quantity = 8, Category = "Mobiliario", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Cable HDMI 2m", Description = "Cable HDMI 2.1 de alta velocidad", Price = 29.90m, Quantity = 200, Category = "Accesorios", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Disco Duro Externo 1TB", Description = "Disco duro portátil USB 3.0 1TB", Price = 219.90m, Quantity = 35, Category = "Almacenamiento", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Webcam HD 1080p", Description = "Cámara web Full HD con micrófono integrado", Price = 189.90m, Quantity = 3, Category = "Periféricos", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Audífonos Bluetooth", Description = "Audífonos over-ear con cancelación de ruido", Price = 449.90m, Quantity = 0, Category = "Audio", CreatedAt = DateTime.UtcNow },
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
                logger.LogInformation("[OK] {Count} productos creados", products.Count);
            }

            logger.LogInformation("[OK] Sistema completado con usuarios y productos");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ERROR] Seeding fallido: {Message}", ex.Message);
            throw;
        }
    }
}
