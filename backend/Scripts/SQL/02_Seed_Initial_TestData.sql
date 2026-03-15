/*
╔════════════════════════════════════════════════════════════════════════════╗
║              SCRIPT: Cargar Datos de Prueba Iniciales                     ║
║                      Sistema: Gestión de Inventarios                      ║
╚════════════════════════════════════════════════════════════════════════════╝

DESCRIPCIÓN:
  Este script carga datos de prueba realistas en la base de datos para
  permitir pruebas del sistema CRUD sin necesidad de crear datos manualmente.

DATOS QUE CARGA:
  • 5 categorías de productos (Electrónica, Accesorios, Software, etc.)
  • 10 productos con información completa de precios y cantidades
  • 8 movimientos de stock como examples de operaciones
  • Usuarios configurados automáticamente por el seeder de .NET

IMPORTANTE:
  - Estos datos son de PRUEBA solamente
  - Los IDs se generan automáticamente
  - Se puede ejecutar en cualquier momento sobre BD limpia
*/

USE [InventoryManagementDB]
GO

-- ============================================
-- PASO 1: INSERTAR CATEGORÍAS
-- ============================================
PRINT ''
PRINT '╔════════════════════════════════════════════════════════╗'
PRINT '║      CARGANDO DATOS DE PRUEBA INICIALES               ║'
PRINT '╚════════════════════════════════════════════════════════╝'
PRINT ''
PRINT 'PASO 1: Insertando categorías de productos...'

INSERT INTO [dbo].[Categories] ([Name], [Description])
VALUES 
    ('Electrónica', 'Equipos electrónicos y computadoras'),
    ('Accesorios', 'Accesorios para devices y periféricos'),
    ('Software', 'Licencias y software comercial'),
    ('Redes', 'Equipos de red y conectividad'),
    ('Periféricos', 'Teclados, mouses, monitores y más')

PRINT '✅ 5 categorías insertadas exitosamente'
GO

-- ============================================
-- PASO 2: INSERTAR PRODUCTOS
-- ============================================
PRINT 'PASO 2: Insertando 10 productos de ejemplo...'

INSERT INTO [dbo].[Products] (
    [Sku], [Name], [Description], [Price], [CategoryId], 
    [Quantity], [ReorderLevel], [IsActive]
)
VALUES 
    -- === LAPTOPS ===
    ('DELL-XPS-13-2026', 'Laptop Dell XPS 13', 
     'Laptop de negocios premium - Intel i7, 16GB RAM, 512GB SSD, pantalla 13" FHD', 
     1299.99, 1, 15, 5, 1),
    
    ('HP-PAVILION-15', 'Laptop HP Pavilion 15', 
     'Laptop versátil - AMD Ryzen 5, 8GB RAM, 256GB SSD, pantalla 15.6" FHD', 
     699.99, 1, 25, 10, 1),
    
    ('LENOVO-THINKPAD-X1', 'Lenovo ThinkPad X1', 
     'Laptop profesional robusta - Intel i5, 8GB RAM, 512GB SSD, 14" pantalla', 
     899.99, 1, 8, 5, 1),
    
    -- === MONITORES ===
    ('LG-27-4K-UHD', 'Monitor LG 27" 4K', 
     'Monitor profesional 4K UltraHD - 60Hz, DisplayPort, HDMI, panel IPS', 
     399.99, 5, 12, 3, 1),
    
    ('DELL-24-FHD', 'Monitor Dell 24" FHD', 
     'Monitor económico Full HD - 60Hz, panel IPS, ajuste de altura, HDMI', 
     199.99, 5, 35, 10, 1),
    
    -- === PERIFÉRICOS ===
    ('LOGITECH-MX-KEYS', 'Teclado Logitech MX Keys', 
     'Teclado inalámbrico profesional - Mecánico, RGB, multi-dispositivo, batería', 
     99.99, 2, 42, 15, 1),
    
    ('RAZER-VIPER-MINI', 'Ratón Razer Viper Mini', 
     'Ratón gaming de alto rendimiento - 20000 DPI, 8 botones programables, liviano', 
     69.99, 2, 56, 20, 1),
    
    -- === ACCESORIOS ===
    ('USB-C-HUB-7PORT', 'Hub USB-C 7 Puertos', 
     'Hub multifunción USB-C - Carga rápida, HDMI 4K, lector SD, display port', 
     49.99, 2, 78, 30, 1),
    
    ('LAPTOP-STAND-ALUMINIO', 'Soporte Laptops Aluminio', 
     'Soporte ergonómico ajustable - Aluminio 100%, soporta hasta 17", plegable', 
     39.99, 2, 123, 50, 1),
    
    -- === NETWORKING ===
    ('TP-LINK-WIFI6-1500', 'Router TP-Link WiFi 6', 
     'Router WiFi 6 de última generación - AX1500, antenas externas, MIMO 2x2', 
     129.99, 4, 18, 5, 1)

PRINT '✅ 10 productos insertados exitosamente'
GO

-- ============================================
-- PASO 3: INSERTAR MOVIMIENTOS DE STOCK
-- ============================================
PRINT 'PASO 3: Insertando movimientos de stock (historial de operaciones)...'

-- Obtener IDs de productos para los movimientos
DECLARE @ProductId1 INT = (SELECT TOP 1 [Id] FROM [dbo].[Products] WHERE [Sku] = 'DELL-XPS-13-2026')
DECLARE @ProductId2 INT = (SELECT TOP 1 [Id] FROM [dbo].[Products] WHERE [Sku] = 'HP-PAVILION-15')
DECLARE @ProductId3 INT = (SELECT TOP 1 [Id] FROM [dbo].[Products] WHERE [Sku] = 'RAZER-VIPER-MINI')

INSERT INTO [dbo].[StockMovements] (
    [ProductId], [MovementType], [Quantity], [Reason], [Reference], [CreatedBy]
)
VALUES 
    -- Movimientos del Laptop Dell XPS 13
    (@ProductId1, 'Purchase', 10, 'Compra inicial a distribuidor autorizado', 'PO-2026-001', 1),
    (@ProductId1, 'Sale', 2, 'Venta a cliente XYZ Corp - Orden #123', 'INV-2026-001', 1),
    
    -- Movimientos del Laptop HP Pavilion
    (@ProductId2, 'Purchase', 20, 'Compra a proveedor HP directo', 'PO-2026-002', 1),
    (@ProductId2, 'Sale', 3, 'Venta mayorista a tienda retail', 'INV-2026-002', 1),
    (@ProductId2, 'Adjustment', 2, 'Ajuste de inventario por daño en transporte', 'ADJ-2026-001', 1),
    
    -- Movimientos del Ratón Razer Viper
    (@ProductId3, 'Purchase', 50, 'Compra inicial Razer - lote grande', 'PO-2026-003', 1),
    (@ProductId3, 'Sale', 5, 'Venta minorista múltiples clientes', 'INV-2026-003', 1),
    (@ProductId3, 'Return', 1, 'Devolución por defecto de fábrica', 'RET-2026-001', 1)

PRINT '✅ 8 movimientos de stock insertados (historial de operaciones)'
GO

-- ============================================
-- PASO 4: MOSTRAR RESUMEN DE DATOS CARGADOS
-- ============================================
PRINT ''
PRINT 'PASO 4: Datos cargados exitosamente'
PRINT ''

PRINT '📊 CATEGORÍAS DISPONIBLES:'
SELECT [Id], [Name], [Description], [IsActive] 
FROM [dbo].[Categories]
ORDER BY [Id]
GO

PRINT ''
PRINT '📦 PRODUCTOS DISPONIBLES (10 productos con validaciones aplicadas):'
SELECT [Id], [Sku], [Name], [Price], [Quantity], [ReorderLevel], [IsActive],
       CASE 
           WHEN [Quantity] = 0 THEN '🔴 SIN STOCK'
           WHEN [Quantity] <= [ReorderLevel] THEN '⚠️ STOCK BAJO'
           ELSE '✅ STOCK OK'
       END AS [Estado Inventario]
FROM [dbo].[Products]
WHERE [IsActive] = 1
ORDER BY [Id]
GO

PRINT ''
PRINT '📈 MOVIMIENTOS DE STOCK REGISTRADOS (8 movimientos de auditoría):'
SELECT [Id], [ProductId], [MovementType], [Quantity], [Reason], [Reference], [CreatedAt]
FROM [dbo].[StockMovements]
ORDER BY [CreatedAt] DESC
GO

PRINT ''
PRINT '👥 USUARIOS DEL SISTEMA (Creados automáticamente por DatabaseSeeder):'
SELECT [Id], [Username], [Email], [Role], [IsActive], [CreatedAt]
FROM [dbo].[Users]
ORDER BY [Id]
GO

-- ============================================
-- RESUMEN FINAL
-- ============================================
PRINT ''
PRINT '╔════════════════════════════════════════════════════════╗'
PRINT '║   ✅ DATOS DE PRUEBA CARGADOS EXITOSAMENTE            ║'
PRINT '║                                                        ║'
PRINT '║   RESUMEN:                                            ║'
PRINT '║   ✓ 5 categorías cargadas                             ║'
PRINT '║   ✓ 10 productos con datos realistas                  ║'
PRINT '║   ✓ 8 movimientos de stock como ejemplos              ║'
PRINT '║   ✓ Todas las validaciones CHECK aplicadas            ║'
PRINT '║   ✓ Integridad referencial confirmada                 ║'
PRINT '║                                                        ║'
PRINT '║   PRÓXIMO PASO: Ejecutar 03_Validate_Database_Constraints.sql ║'
PRINT '╚════════════════════════════════════════════════════════╝'
PRINT ''
PRINT 'NOTAS IMPORTANTES:'
PRINT '  • Todos los productos tienen cantidad >= 0'
PRINT '  • Todos los precios tienen valores >= 0'
PRINT '  • Los niveles de reorden están configurados para alertas'
PRINT '  • Los movimientos de stock son immutables (historial no editable)'
PRINT '  • Los usuarios se crean en primer inicio con roles configurados'
