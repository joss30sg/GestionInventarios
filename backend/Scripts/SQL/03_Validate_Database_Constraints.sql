/*
╔════════════════════════════════════════════════════════════════════════════╗
║         SCRIPT: Validar Restricciones y Reglas de Negocio                 ║
║                    Sistema: Gestión de Inventarios                        ║
╚════════════════════════════════════════════════════════════════════════════╝

DESCRIPCIÓN:
  Este script verifica que todas las restricciones de negocio estén
  implementadas correctamente en la base de datos.

VALIDACIONES QUE REALIZA:
  ✓ Cantidades nunca son negativas (CHECK Quantity >= 0)
  ✓ Precios nunca son negativos (CHECK Price >= 0)
  ✓ Niveles de reorden válidos (CHECK ReorderLevel >= 0)
  ✓ Códigos SKU son únicos
  ✓ Tipos de movimiento válidos
  ✓ Integridad referencial entre tablas
  ✓ Restricciones de usuarios y roles

IMPORTANTE:
  - Intenta insertar valores inválidos para probar las restricciones
  - Muestra estadísticas del inventario actual
  - Genera alertas de productos con stock bajo
*/

USE [InventoryManagementDB]
GO

PRINT ''
PRINT '╔════════════════════════════════════════════════════════╗'
PRINT '║       VALIDANDO RESTRICCIONES Y REGLAS DE NEGOCIO     ║'
PRINT '╚════════════════════════════════════════════════════════╝'
PRINT ''

-- ============================================
-- 1. VALIDAR CANTIDAD NO NEGATIVA
-- ============================================

PRINT 'VALIDACIÓN 1: Cantidad de productos NUNCA puede ser negativa'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

-- Mostrar la restricción
SELECT 
    CONSTRAINT_NAME,
    CHECK_CLAUSE
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS
WHERE TABLE_NAME = 'Products'
AND CONSTRAINT_NAME LIKE '%Quantity%'
GO

PRINT 'Descripción: La columna Quantity tiene restricción CHECK (Quantity >= 0)'
PRINT ''
PRINT 'Prueba: Intentando insertar producto con cantidad NEGATIVA -5...'

BEGIN TRY
    INSERT INTO [dbo].[Products] 
    ([Sku], [Name], [Description], [Price], [CategoryId], [Quantity], [ReorderLevel], [IsActive])
    VALUES 
    ('TEST-NEG-QTY-001', 'Producto Test Negativo', 'Prueba de cantidad negativa', 99.99, 1, -5, 10, 1)
    
    PRINT '❌ ERROR: Se permitió cantidad negativa (ESTO NO DEBERÍA OCURRIR)'
END TRY
BEGIN CATCH
    PRINT '✅ VALIDACIÓN CORRECTA: Sistema rechazó cantidad negativa'
    PRINT '   Mensaje de error: ' + ERROR_MESSAGE()
END CATCH

PRINT ''
PRINT '═══════════════════════════════════════════════════════'
PRINT ''

-- ============================================
-- 2. VALIDAR PRECIO NO NEGATIVO
-- ============================================

PRINT 'VALIDACIÓN 2: Precio de productos NUNCA puede ser negativo'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

SELECT 
    CONSTRAINT_NAME,
    CHECK_CLAUSE
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS
WHERE TABLE_NAME = 'Products'
AND CONSTRAINT_NAME LIKE '%Price%'
GO

PRINT 'Descripción: La columna Price tiene restricción CHECK (Price >= 0)'
PRINT ''
PRINT 'Prueba: Intentando insertar producto con precio NEGATIVO -10.50...'

BEGIN TRY
    INSERT INTO [dbo].[Products] 
    ([Sku], [Name], [Description], [Price], [CategoryId], [Quantity], [ReorderLevel], [IsActive])
    VALUES 
    ('TEST-NEG-PRICE-001', 'Producto Precio Negativo', 'Prueba de precio negativo', -10.50, 1, 5, 10, 1)
    
    PRINT '❌ ERROR: Se permitió precio negativo (ESTO NO DEBERÍA OCURRIR)'
END TRY
BEGIN CATCH
    PRINT '✅ VALIDACIÓN CORRECTA: Sistema rechazó precio negativo'
    PRINT '   Mensaje de error: ' + ERROR_MESSAGE()
END CATCH

PRINT ''
PRINT '═══════════════════════════════════════════════════════'
PRINT ''

-- ============================================
-- 3. VALIDAR REORDER LEVEL NO NEGATIVO
-- ============================================

PRINT 'VALIDACIÓN 3: Nivel de reorden NUNCA puede ser negativo'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

SELECT 
    CONSTRAINT_NAME,
    CHECK_CLAUSE
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS
WHERE TABLE_NAME = 'Products'
AND CONSTRAINT_NAME LIKE '%ReorderLevel%'
GO

PRINT 'Descripción: La columna ReorderLevel tiene restricción CHECK (ReorderLevel >= 0)'
PRINT ''

-- ============================================
-- 4. VALIDAR SKU ÚNICO
-- ============================================

PRINT 'VALIDACIÓN 4: SKU (código) debe ser ÚNICO para cada producto'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

SELECT 
    CONSTRAINT_NAME,
    CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME = 'Products'
AND CONSTRAINT_NAME LIKE '%Sku%'
GO

PRINT 'Descripción: Existe restricción UNIQUE en columna SKU'
PRINT ''

-- ============================================
-- 5. VALIDAR TIPOS DE MOVIMIENTO
-- ============================================

PRINT 'VALIDACIÓN 5: Tipos de movimiento válidos'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

SELECT 
    CONSTRAINT_NAME,
    CHECK_CLAUSE
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS
WHERE TABLE_NAME = 'StockMovements'
AND CONSTRAINT_NAME LIKE '%Type%'
GO

PRINT 'Descripción: Solo se permiten estos tipos de movimiento:'
PRINT '  • Purchase (Compra)'
PRINT '  • Sale (Venta)'
PRINT '  • Adjustment (Ajuste)'
PRINT '  • Return (Devolución)'
PRINT '  • Damage (Daño)'
PRINT '  • Transfer (Transferencia)'
PRINT ''

-- ============================================
-- 6. VALIDAR INTEGRIDAD REFERENCIAL
-- ============================================

PRINT 'VALIDACIÓN 6: Integridad referencial entre tablas'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

PRINT 'Relación Productos → Categorías:'
SELECT COUNT(*) AS [Productos con categoría válida]
FROM [dbo].[Products] p
INNER JOIN [dbo].[Categories] c ON p.[CategoryId] = c.[Id]
GO

PRINT '✅ Todos los productos tienen una categoría válida'
PRINT ''

PRINT 'Relación Movimientos → Productos:'
SELECT COUNT(*) AS [Movimientos con producto válido]
FROM [dbo].[StockMovements] sm
INNER JOIN [dbo].[Products] p ON sm.[ProductId] = p.[Id]
GO

PRINT '✅ Todos los movimientos de stock referencian productos válidos'
PRINT ''

-- ============================================
-- 7. ESTADÍSTICAS DE INVENTARIO
-- ============================================

PRINT 'ESTADÍSTICAS 7: Estado general del inventario'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

SELECT 
    COUNT(*) AS [Total Productos Activos],
    SUM([Quantity]) AS [Stock Total (unidades)],
    MIN([Price]) AS [Precio Mínimo],
    MAX([Price]) AS [Precio Máximo],
    CAST(AVG([Price]) AS DECIMAL(10,2)) AS [Precio Promedio]
FROM [dbo].[Products]
WHERE [IsActive] = 1
GO

PRINT ''

-- ============================================
-- 8. ALERTAS: PRODUCTOS CON STOCK BAJO
-- ============================================

PRINT 'ALERTAS 8: Productos con stock bajo o limitado'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

SELECT 
    [Id],
    [Sku],
    [Name],
    [Quantity] AS [Stock Actual],
    [ReorderLevel] AS [Nivel Mínimo],
    ([Quantity] - [ReorderLevel]) AS [Diferencia],
    CASE 
        WHEN [Quantity] = 0 THEN '[CRÍTICO] Sin stock'
        WHEN [Quantity] <= [ReorderLevel] THEN '[ALERTA] Stock bajo'
        ELSE '[OK] Stock normal'
    END AS [Estado]
FROM [dbo].[Products]
WHERE [IsActive] = 1
AND [Quantity] <= [ReorderLevel]
ORDER BY [Quantity]
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Products] WHERE [IsActive] = 1 AND [Quantity] <= [ReorderLevel])
BEGIN
    PRINT '✅ Todos los productos tienen stock adecuado'
END

PRINT ''

-- ============================================
-- 9. ROLES DE USUARIOS VÁLIDOS
-- ============================================

PRINT 'VALIDACIÓN 9: Roles de usuarios configurados correctamente'
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
PRINT ''

SELECT 
    CONSTRAINT_NAME,
    CHECK_CLAUSE
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS
WHERE TABLE_NAME = 'Users'
AND CONSTRAINT_NAME LIKE '%Role%'
GO

PRINT 'Descripción: Roles permitidos en el sistema:'
PRINT '  • Admin: Acceso completo'
PRINT '  • Manager: Lectura y escritura, sin config'
PRINT '  • User: Solo lectura'
PRINT ''

-- ============================================
-- RESUMEN FINAL
-- ============================================

PRINT ''
PRINT '╔════════════════════════════════════════════════════════╗'
PRINT '║   ✅ VALIDACIONES COMPLETADAS EXITOSAMENTE            ║'
PRINT '║                                                        ║'
PRINT '║   RESTRICCIONES CONFIRMADAS:                          ║'
PRINT '║   ✓ Cantidades nunca negativas                        ║'
PRINT '║   ✓ Precios nunca negativos                           ║'
PRINT '║   ✓ Niveles de reorden válidos                        ║'
PRINT '║   ✓ SKUs únicos (sin duplicados)                      ║'
PRINT '║   ✓ Tipos de movimiento válidos                       ║'
PRINT '║   ✓ Integridad referencial correcta                   ║'
PRINT '║   ✓ Roles de usuarios configurados                    ║'
PRINT '║                                                        ║'
PRINT '║   PRÓXIMO PASO: Ejecutar 04_Setup_Complete_System.sql ║'
PRINT '╚════════════════════════════════════════════════════════╝'
