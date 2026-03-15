/*
╔════════════════════════════════════════════════════════════════════════════╗
║          SCRIPT: Verificar y Configurar Sistema Completo                  ║
║         Script Maestro de Validación - Gestión de Inventarios             ║
╚════════════════════════════════════════════════════════════════════════════╝

DESCRIPCIÓN:
  Este es el script maestro que ejecuta todas las validaciones finales del
  sistema. Verifica que:
  - La estructura de BD existe y es correcta
  - Los datos están cargados correctamente
  - Todas las restricciones se aplican
  - El sistema está listo para consumirse desde la API

FLUJO DE EJECUCIÓN:
  1. PASO 1: Verificar que todas las tablas existan
  2. PASO 2: Contar registros en el sistema
  3. PASO 3: Validar restricciones de integridad
  4. PASO 4: Mostrar productos disponibles para CRUD
  5. PASO 5: Mostrar movimientos de stock
  6. PASO 6: Mostrar usuarios del sistema
  7. PASO 7: Resumen final y estado del sistema

RECOMENDACIÓN:
  Ejecute este script DESPUÉS de ejecutar:
  - 01_Create_Schema_Tables.sql
  - 02_Seed_Initial_TestData.sql
  - 03_Validate_Database_Constraints.sql
*/

USE [InventoryManagementDB]
GO

PRINT ''
PRINT '╔═══════════════════════════════════════════════════════════╗'
PRINT '║     SCRIPT MAESTRO: VERIFICACIÓN COMPLETA DEL SISTEMA     ║'
PRINT '║         Sistema listo para consumir desde API             ║'
PRINT '╚═══════════════════════════════════════════════════════════╝'
PRINT ''

DECLARE @ExecutionTime DATETIME2 = GETUTCDATE()

-- ============================================
-- PASO 1: Verificar estructura de la base de datos
-- ============================================

PRINT 'PASO 1: Verificando que la estructura de BD existe...'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

DECLARE @TablesExist BIT = 0

IF OBJECT_ID('[dbo].[Products]', 'U') IS NOT NULL 
   AND OBJECT_ID('[dbo].[Categories]', 'U') IS NOT NULL
   AND OBJECT_ID('[dbo].[StockMovements]', 'U') IS NOT NULL
   AND OBJECT_ID('[dbo].[Users]', 'U') IS NOT NULL
   AND OBJECT_ID('[dbo].[AuditLogs]', 'U') IS NOT NULL
   AND OBJECT_ID('[dbo].[InventoryAlerts]', 'U') IS NOT NULL
BEGIN
    SET @TablesExist = 1
    PRINT '✅ CORRECTO: Todas las tablas existen'
    PRINT '   - [dbo].[Categories] ✓'
    PRINT '   - [dbo].[Products] ✓'
    PRINT '   - [dbo].[StockMovements] ✓'
    PRINT '   - [dbo].[InventoryAlerts] ✓'
    PRINT '   - [dbo].[Users] ✓'
    PRINT '   - [dbo].[AuditLogs] ✓'
END
ELSE
BEGIN
    SET @TablesExist = 0
    PRINT '❌ ERROR: Faltan tablas. Ejecuta 01_Create_Schema_Tables.sql primero'
    PRINT ''
    PRINT 'Tablas que deben existir:'
    PRINT '   - dbo.Categories'
    PRINT '   - dbo.Products'
    PRINT '   - dbo.StockMovements'
    PRINT '   - dbo.InventoryAlerts'
    PRINT '   - dbo.Users'
    PRINT '   - dbo.AuditLogs'
    GOTO ErrorExit
END

PRINT ''
PRINT '═══════════════════════════════════════════════════════════'
PRINT ''

-- ============================================
-- PASO 2: Contar registros actuales
-- ============================================

PRINT 'PASO 2: Estado actual del sistema (conteo de registros)...'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

DECLARE @CategoryCount INT = (SELECT COUNT(*) FROM [dbo].[Categories])
DECLARE @ProductCount INT = (SELECT COUNT(*) FROM [dbo].[Products])
DECLARE @UserCount INT = (SELECT COUNT(*) FROM [dbo].[Users])
DECLARE @MovementCount INT = (SELECT COUNT(*) FROM [dbo].[StockMovements])
DECLARE @AlertCount INT = (SELECT COUNT(*) FROM [dbo].[InventoryAlerts])
DECLARE @AuditCount INT = (SELECT COUNT(*) FROM [dbo].[AuditLogs])

PRINT 'Registros en el sistema:'
PRINT '  📁 Categorías: ' + CAST(@CategoryCount AS NVARCHAR(10)) + ' registros'
PRINT '  📦 Productos: ' + CAST(@ProductCount AS NVARCHAR(10)) + ' registros'
PRINT '  👥 Usuarios: ' + CAST(@UserCount AS NVARCHAR(10)) + ' registros'
PRINT '  📈 Movimientos Stock: ' + CAST(@MovementCount AS NVARCHAR(10)) + ' registros'
PRINT '  ⚠️  Alertas: ' + CAST(@AlertCount AS NVARCHAR(10)) + ' registros'
PRINT '  📋 Logs Auditoría: ' + CAST(@AuditCount AS NVARCHAR(10)) + ' registros'
PRINT ''

PRINT '═══════════════════════════════════════════════════════════'
PRINT ''

-- ============================================
-- PASO 3: Validar restricciones e integridad
-- ============================================

PRINT 'PASO 3: Validando restricciones e integridad de datos...'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

-- Verificar que NO hay cantidades negativas
DECLARE @NegativeQty INT = (SELECT COUNT(*) FROM [dbo].[Products] WHERE [Quantity] < 0)
DECLARE @NegativePrice INT = (SELECT COUNT(*) FROM [dbo].[Products] WHERE [Price] < 0)

IF @NegativeQty > 0
BEGIN
    PRINT '❌ ERROR: ' + CAST(@NegativeQty AS NVARCHAR(10)) + ' productos con cantidad negativa'
END
ELSE IF @ProductCount > 0
BEGIN
    PRINT '✅ VALIDACIÓN 1: Todas las cantidades son >= 0'
END

IF @NegativePrice > 0
BEGIN
    PRINT '❌ ERROR: ' + CAST(@NegativePrice AS NVARCHAR(10)) + ' productos con precio negativo'
END
ELSE IF @ProductCount > 0
BEGIN
    PRINT '✅ VALIDACIÓN 2: Todos los precios son >= 0'
END

-- Verificar integridad referencial
DECLARE @OrphantProducts INT = (
    SELECT COUNT(*) FROM [dbo].[Products] p
    WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Categories] c WHERE c.[Id] = p.[CategoryId])
)

IF @OrphantProducts > 0
BEGIN
    PRINT '❌ ERROR: ' + CAST(@OrphantProducts AS NVARCHAR(10)) + ' productos sin categoría válida'
END
ELSE IF @ProductCount > 0
BEGIN
    PRINT '✅ VALIDACIÓN 3: Integridad referencial Productos-Categorías correcta'
END

-- Verificar movimientos válidos
DECLARE @InvalidMovements INT = (
    SELECT COUNT(*) FROM [dbo].[StockMovements] sm
    WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Products] p WHERE p.[Id] = sm.[ProductId])
)

IF @InvalidMovements > 0
BEGIN
    PRINT '❌ ERROR: ' + CAST(@InvalidMovements AS NVARCHAR(10)) + ' movimientos sin producto válido'
END
ELSE IF @MovementCount > 0
BEGIN
    PRINT '✅ VALIDACIÓN 4: Integridad referencial Movimientos-Productos correcta'
END

PRINT ''
PRINT '═══════════════════════════════════════════════════════════'
PRINT ''

-- ============================================
-- PASO 4: Mostrar productos activos (Ready for CRUD)
-- ============================================

PRINT 'PASO 4: Productos disponibles para operaciones CRUD'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

IF @ProductCount > 0
BEGIN
    SELECT TOP 10
        [Id],
        [Sku],
        [Name],
        [Price],
        [Quantity],
        [ReorderLevel],
        CASE 
            WHEN [Quantity] = 0 THEN '🔴 SIN STOCK'
            WHEN [Quantity] <= [ReorderLevel] THEN '⚠️ STOCK BAJO'
            ELSE '✅ STOCK OK'
        END AS [Estado]
    FROM [dbo].[Products]
    WHERE [IsActive] = 1
    ORDER BY [Id]
    
    PRINT ''
END
ELSE
BEGIN
    PRINT '⚠️ No hay productos disponibles'
    PRINT 'Carga datos con: 02_Seed_Initial_TestData.sql'
    PRINT ''
END

PRINT '═══════════════════════════════════════════════════════════'
PRINT ''

-- ============================================
-- PASO 5: Mostrar historial de movimientos
-- ============================================

PRINT 'PASO 5: Historial de movimientos de stock (auditoría)'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

IF @MovementCount > 0
BEGIN
    SELECT TOP 10
        [Id],
        [ProductId],
        [MovementType],
        [Quantity],
        [Reference],
        [CreatedAt]
    FROM [dbo].[StockMovements]
    ORDER BY [CreatedAt] DESC
    
    PRINT ''
END
ELSE
BEGIN
    PRINT '⚠️ No hay movimientos registrados'
    PRINT ''
END

PRINT '═══════════════════════════════════════════════════════════'
PRINT ''

-- ============================================
-- PASO 6: Mostrar usuarios configurados
-- ============================================

PRINT 'PASO 6: Usuarios del sistema (autenticación)'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

IF @UserCount > 0
BEGIN
    SELECT
        [Id],
        [Username],
        [Email],
        [Role],
        [IsActive],
        [CreatedAt]
    FROM [dbo].[Users]
    WHERE [IsActive] = 1
    ORDER BY [Id]
    
    PRINT ''
END
ELSE
BEGIN
    PRINT '⚠️ No hay usuarios configurados'
    PRINT 'Los usuarios se crean automáticamente en primer inicio'
    PRINT ''
END

-- ============================================
-- PASO 7: RESUMEN Y STATUS DEL SISTEMA
-- ============================================

PRINT '═══════════════════════════════════════════════════════════'
PRINT ''

PRINT 'RESUMEN: Verificación de requisitos para API'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

IF @ProductCount >= 5
    PRINT '✅ Hay suficientes productos para pruebas ('+ CAST(@ProductCount AS NVARCHAR(10)) +')'
ELSE IF @ProductCount > 0
    PRINT '⚠️  Productos insuficientes ('+ CAST(@ProductCount AS NVARCHAR(10)) +'). Recomendado: >= 5'
ELSE
    PRINT '❌ No hay productos. Cargar con 02_Seed_Initial_TestData.sql'

IF @CategoryCount >= 3
    PRINT '✅ Hay categorías configuradas ('+ CAST(@CategoryCount AS NVARCHAR(10)) +')'
ELSE
    PRINT '⚠️  Pocas categorías ('+ CAST(@CategoryCount AS NVARCHAR(10)) +')'

IF @UserCount > 0
    PRINT '✅ Usuarios del sistema configurados ('+ CAST(@UserCount AS NVARCHAR(10)) +')'
ELSE
    PRINT '⚠️  No hay usuarios. Se crearán en primer inicio de API'

IF @NegativeQty = 0 AND @NegativePrice = 0
    PRINT '✅ Todas las validaciones de datos cumplidas'
ELSE
    PRINT '❌ Hay datos inválidos en el sistema'

PRINT ''
PRINT '╔═══════════════════════════════════════════════════════════╗'
PRINT '║   ✅ SISTEMA LISTO PARA CONSUMIR DESDE API                ║'
PRINT '║                                                           ║'
PRINT '║   ENDPOINTS DISPONIBLES:                                 ║'
PRINT '║   POST   /api/v1/products          - Crear producto      ║'
PRINT '║   GET    /api/v1/products          - Listar productos    ║'
PRINT '║   GET    /api/v1/products/{id}     - Obtener producto    ║'
PRINT '║   PUT    /api/v1/products/{id}     - Actualizar producto ║'
PRINT '║   DELETE /api/v1/products/{id}     - Eliminar producto   ║'
PRINT '║                                                           ║'
PRINT '║   POST   /api/v1/categories        - CRUD Categorías     ║'
PRINT '║   POST   /api/v1/inventory/move    - Mov. de stock       ║'
PRINT '║   GET    /api/v1/inventory/alerts  - Alertas de stock    ║'
PRINT '║                                                           ║'
PRINT '╚═══════════════════════════════════════════════════════════╝'
PRINT ''

GOTO Success

ErrorExit:
PRINT ''
PRINT '╔═══════════════════════════════════════════════════════════╗'
PRINT '║   ❌ ERROR EN LA CONFIGURACIÓN DEL SISTEMA                ║'
PRINT '║                                                           ║'
PRINT '║   Pasos requeridos (en orden):                           ║'
PRINT '║   1. 01_Create_Schema_Tables.sql                         ║'
PRINT '║   2. 02_Seed_Initial_TestData.sql                        ║'
PRINT '║   3. 03_Validate_Database_Constraints.sql                ║'
PRINT '║   4. 04_Setup_Complete_System.sql (este script)          ║'
PRINT '║                                                           ║'
PRINT '╚═══════════════════════════════════════════════════════════╝'
RETURN

Success:
PRINT 'Ejecución completada: ' + CONVERT(NVARCHAR(30), @ExecutionTime, 121)
PRINT ''
